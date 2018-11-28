using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CommonApi.Errors;
using CommonApi.Resopnse;
using Dal;
using Infrastructure.Entities;
using Infrastructure.Model.Provider;
using Infrastructure.Model.User;
using Infrastructure.System.Options;
using Microsoft.AspNetCore.Http;

namespace Manager
{
    public class ProviderUserManager : BaseManager
    {
        protected readonly ProviderRepository _providerRepository;
        protected readonly AppUserRepository _appUserRepository;

        public ProviderUserManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper,
            ProviderRepository providerRepository, AppUserRepository appUserRepository) : base(httpContextAccessor, appUserManager, mapper)
        {
            _providerRepository = providerRepository;
            _appUserRepository = appUserRepository;
        }

        public async Task<ApiResponse<UserDisplayModel>> InviteUser(ProviderUserCreateModel model)
        {
            var currentUser = await GetCurrentUser();

            if (!await _providerRepository.ExistsWithOwner(model.ProviderId, currentUser.Id))
                return Fail(ECollection.ACCESS_DENIED);

            if (model.Roles == null || !model.Roles.Any())
                return Fail(ECollection.Select(ECollection.PROPERTY_REQUIRED, "Roles are required"));

            var pShort = await _providerRepository.GetShortById(model.ProviderId);
            var roles = await _providerRepository.GetRolesById(model.ProviderId);

            var rolesToUse = roles.Where(x => model.Roles.Contains(x.Id)).ToList();

            var existsUser = await _appUserRepository.FindByEmailAsync(model.Email.ToLower(), new System.Threading.CancellationToken());
            if(existsUser == null)
            {
                existsUser = new Infrastructure.Entities.AppUser
                {
                    Email = model.Email.ToLower(),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Tels = new List<MRDbIdentity.Domain.UserTel>(),
                    Status = Infrastructure.Entities.UserStatus.Invited,
                    UserName = model.Email.ToLower(),
                };

                existsUser.ConnectedProviders = new List<Infrastructure.Entities.AppUserProvider>
                {
                    new Infrastructure.Entities.AppUserProvider
                    {
                        ProviderId = pShort.Id,
                        ProviderName = pShort.Name,
                        Roles = rolesToUse,
                        Metadata = new List<Infrastructure.Entities.AppUserProviderMeta>(),
                        UpdatedTime = DateTime.UtcNow
                    }
                };

                var insertResult = await _appUserManager.CreateAsync(existsUser);

                if (!insertResult.Succeeded)
                    return Fail(ECollection.UNDEFINED_ERROR);

                await _appUserManager.AddToRoleAsync(existsUser, AppUserRoleList.USER);

                // TODO send hello email
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

                // TODO send hello email
            }
            else
            {
                return Fail(ECollection.USER_ALREADY_CONNECTED);
            }

            var result = _mapper.Map<UserDisplayModel>(existsUser);
            return Ok(result);
        }
    }
}
