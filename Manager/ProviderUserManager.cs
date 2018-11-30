using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CommonApi.Errors;
using CommonApi.Resopnse;
using CommonApi.Response;
using Dal;
using Dal.Tasks;
using Infrastructure.Entities;
using Infrastructure.Model.Provider;
using Infrastructure.Model.User;
using Infrastructure.System.Options;
using Microsoft.AspNetCore.Http;
using Tools;

namespace Manager
{
    public class ProviderUserManager : BaseManager
    {
        protected readonly ProviderRepository _providerRepository;
        protected readonly AppUserRepository _appUserRepository;
        protected readonly EmailSendTaskRepository _emailSendTaskRepository;
        protected readonly TemplateParser _templateParser;

        public ProviderUserManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper,
            ProviderRepository providerRepository, AppUserRepository appUserRepository, EmailSendTaskRepository emailSendTaskRepository, TemplateParser templateParser) : base(httpContextAccessor, appUserManager, mapper)
        {
            _providerRepository = providerRepository;
            _appUserRepository = appUserRepository;
            _emailSendTaskRepository = emailSendTaskRepository;
            _templateParser = templateParser;
        }

        /// <summary>
        /// Invite new user or connect exists
        /// </summary>
        /// <param name="model">Provider create user model</param>
        /// <returns>UserDisplayModel</returns>
        public async Task<ApiResponse<UserDisplayModel>> InviteUser(ProviderUserCreateModel model)
        {
            var currentUser = await GetCurrentUser();

            if (!await _providerRepository.ExistsWithOwner(model.ProviderId, currentUser.Id))
                return Fail(ECollection.ACCESS_DENIED);

            if (model.Roles == null || !model.Roles.Any())
                return Fail(ECollection.Select(ECollection.PROPERTY_REQUIRED, "Roles are required"));

            model.Roles = model.Roles.Select(x => x.ToUpper()).ToList();

            var pShort = await _providerRepository.GetShortById(model.ProviderId);
            var roles = await _providerRepository.GetRolesById(model.ProviderId);

            var rolesToUse = roles.Where(x => model.Roles.Contains(x.Name)).ToList();

            var existsUser = await _appUserRepository.FindByEmailAsync(model.Email.ToLower(), new System.Threading.CancellationToken());
            if(existsUser == null)
            {
                existsUser = new AppUser
                {
                    Email = model.Email.ToLower(),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Tels = new List<MRDbIdentity.Domain.UserTel>(),
                    Status = UserStatus.Invited,
                    UserName = model.Email.ToLower(),
                };

                existsUser.ConnectedProviders = new List<AppUserProvider>
                {
                    new AppUserProvider
                    {
                        ProviderId = pShort.Id,
                        ProviderName = pShort.Name,
                        Roles = rolesToUse,
                        Metadata = new List<AppUserProviderMeta>(),
                        UpdatedTime = DateTime.UtcNow
                    }
                };

                var insertResult = await _appUserManager.CreateAsync(existsUser);

                if (!insertResult.Succeeded)
                    return Fail(ECollection.UNDEFINED_ERROR);

                await _appUserManager.AddToRoleAsync(existsUser, AppUserRoleList.USER);


            }
            else if(!existsUser.ConnectedProviders.Any(x => x.ProviderId == pShort.Id))
            {
                AppUserProvider provider = new AppUserProvider
                {
                    ProviderId = pShort.Id,
                    Metadata = new List<AppUserProviderMeta>(),
                    ProviderName = pShort.Name,
                    Roles = rolesToUse,
                    UpdatedTime = DateTime.UtcNow
                };

                if ((await _appUserRepository.AddProvider(existsUser.Id, provider)).ModifiedCount != 1)
                    return Fail(ECollection.UNDEFINED_ERROR);

            }
            else
            {
                return Fail(ECollection.USER_ALREADY_CONNECTED);
            }

            Infrastructure.Model.Template.User.ProviderInviteTemplate templateModel = new Infrastructure.Model.Template.User.ProviderInviteTemplate
            {
                FirstName = existsUser.FirstName,
                LastName = existsUser.LastName,
                LoginLink = "https://google.com",
                ProviderName = pShort.Name
            };

            var email = await _templateParser.Render("Email", "ProviderInvite", templateModel);
            await _emailSendTaskRepository.InsertEmail(existsUser.Email, "Mad Rat Studio invite", email, Infrastructure.Entities.Tasks.EmailTaskBot.MadRatBot);

            var result = _mapper.Map<UserDisplayModel>(existsUser);
            return Ok(result);
        }

        /// <summary>
        /// Return users connected to provider
        /// </summary>
        /// <param name="providerId">Target provider id</param>
        /// <param name="skip">Skip</param>
        /// <param name="limit">Limit</param>
        /// <returns>List response</returns>
        public async Task<ApiListResponse<UserDisplayModel>> Get(int skip, int limit, string providerId)
        {
            var currentUser = await GetCurrentUser();

            if (!await _providerRepository.ExistsWithOwner(providerId, currentUser.Id))
                return FailList<UserDisplayModel>(ECollection.ACCESS_DENIED);

            var connectedUsers = (await _appUserRepository.GetByProvider(skip, limit, providerId))?.ToList() ?? new List<AppUser>();

            return new ApiListResponse<UserDisplayModel>(connectedUsers.Select(x => _mapper.Map<UserDisplayModel>(x)).ToList(), skip, limit, 0);
        }

        /// <summary>
        /// Update provider user role
        /// </summary>
        /// <param name="model">Provider user update model</param>
        /// <returns>Ok</returns>
        public async Task<ApiResponse> UpdateUserRoles(ProviderUserUpdateModel model)
        {
            var currentUser = await GetCurrentUser();

            if (!await _providerRepository.ExistsWithOwner(model.ProviderId, currentUser.Id))
                return Fail(ECollection.ACCESS_DENIED);

            if (model.Roles == null || !model.Roles.Any())
                return Fail(ECollection.Select(ECollection.PROPERTY_REQUIRED, "Roles are required"));

            model.Roles = model.Roles.Select(x => x.ToUpper()).ToList();

            var pShort = await _providerRepository.GetShortById(model.ProviderId);
            var roles = await _providerRepository.GetRolesById(model.ProviderId);

            var rolesToUse = roles.Where(x => model.Roles.Contains(x.Name)).ToList();

            var targetUser = await _appUserRepository.FindByEmailAsync(model.UserId, new System.Threading.CancellationToken());
            if (targetUser == null)
                return Fail(ECollection.USER_NOT_FOUND);

            if (!(targetUser.ConnectedProviders?.Any(x => x.ProviderId == model.ProviderId) ?? false))
                return Fail(ECollection.PROVIDER_NOT_FOUND);


            await _appUserRepository.UpdateProviderRoles(targetUser.Id, model.ProviderId, rolesToUse);

            return Ok();
        }
    }
}
