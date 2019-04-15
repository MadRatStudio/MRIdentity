using AutoMapper;
using Dal;
using Dal.Tasks;
using Infrastructure.Entities;
using Infrastructure.Model.User;
using Infrastructure.System.Appsettings;
using Infrastructure.Template.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MRDbIdentity.Domain;
using MRIdentityClient.Exception.Basic;
using MRIdentityClient.Exception.Common;
using MRIdentityClient.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Manager
{
    public class AccountManager : BaseManager
    {
        protected readonly AppUserRepository _appUserRepository;
        protected readonly ImageTmpBucket _imageTmpBucket;
        protected readonly ImageOriginBucket _imageOriginBucket;
        protected readonly UrlRedirectSettings _urlRedirectSettings;
        protected readonly TemplateParser _templateParser;
        protected readonly EmailSendTaskRepository _emailSendTaskRepository;

        public AccountManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager,
            IMapper mapper, ILoggerFactory loggerFactory, AppUserRepository appUserRepository,
            ImageTmpBucket imageTmpBucket, ImageOriginBucket imageOriginBucket,
             UrlRedirectSettings urlRedirectSettings, TemplateParser templateParser, EmailSendTaskRepository emailSendTaskRepository) : base(httpContextAccessor, appUserManager, mapper, loggerFactory)
        {
            _appUserRepository = appUserRepository;
            _imageTmpBucket = imageTmpBucket;
            _imageOriginBucket = imageOriginBucket;
            _urlRedirectSettings = urlRedirectSettings;
            _templateParser = templateParser;
            _emailSendTaskRepository = emailSendTaskRepository;
        }

        /// <summary>
        /// Get current user display model
        /// </summary>
        /// <returns>Api respones user display model</returns>
        public async Task<UserDisplayModel> Get()
        {
            var user = await GetCurrentUser();
            if (user == null)
                throw new MRException();

            if (user.Status == Infrastructure.Entities.UserStatus.Blocked)
                throw new MRException();

            return _mapper.Map<UserDisplayModel>(user);
        }

        /// <summary>
        /// Update current user model
        /// </summary>
        /// <param name="model"></param>
        /// <returns>ApiResponse</returns>
        public async Task<UserDisplayModel> Update(UserUpdateModel model)
        {
            var user = await GetCurrentUser();
            if (user == null)
                throw new MRException();

            if (user.Status == UserStatus.Blocked)
                throw new MRException(-1, "User blocked");

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UpdatedTime = DateTime.UtcNow;

            var updateResult = await _appUserRepository.Replace(user);
            if (updateResult == null)
                throw new MRException();

            user = await _appUserRepository.FindByIdAsync(user.Id, new System.Threading.CancellationToken());

            return _mapper.Map<UserDisplayModel>(user);
        }

        /// <summary>
        /// Add tel to current user
        /// </summary>
        /// <param name="model">Create user tel model</param>
        /// <returns>ApiResponse</returns>
        public async Task<ApiOkResult> AddTel(CreateUserTelModel model)
        {
            var user = await GetCurrentUser();
            if (user == null)
                throw new MRException();

            if (user.Status == UserStatus.Blocked)
                throw new MRException();

            if (user.Tels == null)
            {
                user.Tels = new List<UserTel>();
            }
            else
            {
                if (user.Tels.Any(x => x.Name.ToLower() == model.Name.ToLower()))
                    throw new EntityExistsException("Name", model.Name, typeof(string), "Tel with this name already exists");
            }

            user.Tels.Add(new UserTel
            {
                CreatedTime = DateTime.UtcNow,
                Name = model.Name,
                Number = model.Number
            });

            var updateResponse = await _appUserRepository.Replace(user);
            if (updateResponse == null)
                throw new MRException();

            return new ApiOkResult(true);
        }

        /// <summary>
        /// Delete tel
        /// </summary>
        /// <param name="name">Name of target tel</param>
        /// <returns>Api response</returns>
        public async Task<ApiOkResult> DeleteTel(string name)
        {
            var user = await GetCurrentUser();
            if (user == null)
                throw new MRException();

            if (user.Status == UserStatus.Blocked)
                throw new MRException();

            if (user.Tels != null && user.Tels.Any())
            {
                if (user.Tels.Any(x => x.Name.ToLower() == name.ToLower()))
                {
                    user.Tels.RemoveAll(x => x.Name.ToLower() == name.ToLower());

                    await _appUserRepository.Replace(user);
                }
            }

            return new ApiOkResult();
        }

        /// <summary>
        /// Update user email
        /// </summary>
        /// <param name="model">Update email model</param>
        /// <returns>Api response</returns>
        public async Task<ApiOkResult> UpdateEmail(UpdateEmailModel model)
        {
            var user = await GetCurrentUser();
            if (user == null)
                throw new MRException();

            if (user.Status == UserStatus.Blocked)
                throw new MRException();

            user.Email = model.Email;

            var updateResponse = await _appUserRepository.Replace(user);
            if (updateResponse == null)
                throw new MRException();

            return new ApiOkResult();
        }


        /// <summary>
        /// Close current user account
        /// </summary>
        /// <returns>Api response</returns>
        public async Task<ApiOkResult> CloseAccount()
        {
            var user = await GetCurrentUser();
            if (user == null)
                throw new MRException();

            if (user.Status == UserStatus.Blocked)
                throw new MRException();

            user.State = false;
            await _appUserRepository.RemoveSoft(user.Id);

            return new ApiOkResult();
        }
    }
}
