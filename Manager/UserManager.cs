using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CommonApi.Email;
using CommonApi.Email.User;
using CommonApi.Errors;
using CommonApi.Exception.Common;
using CommonApi.Exception.Request;
using CommonApi.Exception.User;
using CommonApi.Resopnse;
using CommonApi.Response;
using Dal;
using Dal.Tasks;
using Infrastructure.Entities;
using Infrastructure.Model.User;
using Manager.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MRDbIdentity.Infrastructure.Interface;
using MRDbIdentity.Service;
using Tools;

namespace Manager
{
    public class UserManager : BaseManager
    {
        protected readonly AppUserRepository _appUserRepository;
        protected readonly EmailSendTaskRepository _emailSendTaskRepository;
        protected readonly TemplateParser _templateParser;

        public UserManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper,
            ILoggerFactory loggerFactory, AppUserRepository appUserRepository, EmailSendTaskRepository emailSendTaskRepository,
            TemplateParser templateParser) : base(httpContextAccessor, appUserManager, mapper, loggerFactory)
        {
            _appUserRepository = appUserRepository;
            _emailSendTaskRepository = emailSendTaskRepository;
            _templateParser = templateParser;
        }

        public async Task<List<string>> GetRoles(string id)
        {
            var user = await _appUserManager.FindByIdAsync(id);
            if (user == null)
                throw new EntityNotFoundException(id, typeof(AppUser));

            return user.Roles.Select(x => x.RoleName).ToList();
        }

        #region admin

        public async Task<ApiListResponse<UserShortDataModel>> AdminGetCollection(int skip, int limit, string q)
        {
            if (skip < 0) skip = 0;
            if (limit > MAX_LIMIT) limit = MAX_LIMIT;
            else if (limit < 0) limit = 1;

            var list = await _appUserRepository.Get(skip, limit, q);
            var result = new ApiListResponse<UserShortDataModel>
            {
                Skip = skip,
                Limit = limit,
                Total = await _appUserRepository.Count(),
                Data = list?.Select(x => _mapper.Map<UserShortDataModel>(x)).ToList() ?? new List<UserShortDataModel>()
            };

            return result;
        }

        public async Task<UserDataModel> AdminGetUserById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new BadRequestException();

            var entity = await _appUserRepository.GetByIdAdmin(id);
            if (entity == null)
                throw new EntityNotFoundException(id, typeof(AppUser));

            return _mapper.Map<UserDataModel>(entity);
        }

        public async Task<UserShortDataModel> AdminCreate(UserCreateModel model)
        {
            if (model == null)
                throw new ModelDamagedException(nameof(model), "is required");

            if((await _appUserManager.FindByEmailAsync(model.Email)) != null)
                throw new EntityExistsException(nameof(model.Email), model.Email, typeof(AppUser));

            var user = new AppUser
            {
                Birthday = new DateTime(),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Tels = model.Tels?.Select((x) => new MRDbIdentity.Domain.UserTel
                {
                    CreatedTime = DateTime.UtcNow,
                    Name = x.Name,
                    Number = x.Number
                }).ToList() ?? new List<MRDbIdentity.Domain.UserTel>(),
                UserName = model.Email
            };

            var createResult = await _appUserManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                throw new SystemException("Can not create user");
            }

            await _appUserRepository.AddToRoleAsync(user, "USER", new System.Threading.CancellationToken());

            UserCreateTemplateModel templateModel = new UserCreateTemplateModel
            {
                Provider = "MR Identity",
                CallbackUrl = "https://google.com",
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            var emailBody = await _templateParser.Render(null, EmailTemplateCollection.USER_INVITE, templateModel);
            await _emailSendTaskRepository.InsertEmail(user.Email, "MR identity invite", emailBody, Infrastructure.Entities.Tasks.EmailTaskBot.MadRatBot);

            return _mapper.Map<UserShortDataModel>(user);
        }

        public async Task<bool> AdminDelete(string id)
        {
            var user = await _appUserManager.FindByIdAsync(id);
            if (user == null)
                throw new EntityNotFoundException(id, typeof(AppUser));

            if (id == (await GetCurrentUser()).Id)
                throw new SystemException("Can not delete self");

            var result = await _appUserManager.DeleteAsync(user);
            return result.Succeeded;
        }

        #endregion

    }
}
