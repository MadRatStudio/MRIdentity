﻿using AutoMapper;
using CommonApi.Email;
using CommonApi.Errors;
using CommonApi.Exception.Common;
using CommonApi.Exception.MRSystem;
using CommonApi.Resopnse;
using CommonApi.Response;
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
        public async Task<ApiResponse<UserDisplayModel>> Get()
        {
            var user = await GetCurrentUser();
            if (user == null)
                return Fail(ECollection.NOT_AUTHORIZED);

            if (user.Status == Infrastructure.Entities.UserStatus.Blocked)
                return Fail(ECollection.USER_BLOCKED);

            return Ok(_mapper.Map<UserDisplayModel>(user));
        }

        /// <summary>
        /// Update current user model
        /// </summary>
        /// <param name="model"></param>
        /// <returns>ApiResponse</returns>
        public async Task<ApiResponse> Update(UserUpdateModel model)
        {
            var user = await GetCurrentUser();
            if (user == null)
                return Fail(ECollection.NOT_AUTHORIZED);

            if (user.Status == UserStatus.Blocked)
                return Fail(ECollection.USER_BLOCKED);

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UpdatedTime = DateTime.UtcNow;

            var updateResult = await _appUserRepository.Replace(user);
            if (updateResult == null)
                return Fail(ECollection.UNDEFINED_ERROR);

            return Ok();
        }

        /// <summary>
        /// Add tel to current user
        /// </summary>
        /// <param name="model">Create user tel model</param>
        /// <returns>ApiResponse</returns>
        public async Task<ApiResponse> AddTel(CreateUserTelModel model)
        {
            var user = await GetCurrentUser();
            if (user == null)
                return Fail(ECollection.NOT_AUTHORIZED);

            if (user.Status == UserStatus.Blocked)
                return Fail(ECollection.USER_BLOCKED);

            if (user.Tels == null)
            {
                user.Tels = new List<UserTel>();
            }
            else
            {
                if (user.Tels.Any(x => x.Name.ToLower() == model.Name.ToLower()))
                    return Fail(ECollection.ENTITY_EXISTS);
            }

            user.Tels.Add(new UserTel
            {
                CreatedTime = DateTime.UtcNow,
                Name = model.Name,
                Number = model.Number
            });

            var updateResponse = await _appUserRepository.Replace(user);
            if (updateResponse == null)
                return Fail(ECollection.UNDEFINED_ERROR);

            return Ok();
        }

        /// <summary>
        /// Delete tel
        /// </summary>
        /// <param name="name">Name of target tel</param>
        /// <returns>Api response</returns>
        public async Task<ApiResponse> DeleteTel(string name)
        {
            var user = await GetCurrentUser();
            if (user == null)
                return Fail(ECollection.NOT_AUTHORIZED);

            if (user.Status == UserStatus.Blocked)
                return Fail(ECollection.USER_BLOCKED);

            if (user.Tels != null && user.Tels.Any())
            {
                if (user.Tels.Any(x => x.Name.ToLower() == name.ToLower()))
                {
                    user.Tels.RemoveAll(x => x.Name.ToLower() == name.ToLower());

                    await _appUserRepository.Replace(user);
                }
            }

            return Ok();
        }

        /// <summary>
        /// Update user email
        /// </summary>
        /// <param name="model">Update email model</param>
        /// <returns>Api response</returns>
        public async Task<ApiResponse> UpdateEmail(UpdateEmailModel model)
        {
            var user = await GetCurrentUser();
            if (user == null)
                return Fail(ECollection.NOT_AUTHORIZED);

            if (user.Status == UserStatus.Blocked)
                return Fail(ECollection.USER_BLOCKED);

            user.Email = model.Email;

            var updateResponse = await _appUserRepository.Replace(user);
            if (updateResponse == null)
                return Fail(ECollection.UNDEFINED_ERROR);

            return Ok();
        }


        /// <summary>
        /// Close current user account
        /// </summary>
        /// <returns>Api response</returns>
        public async Task<ApiResponse> CloseAccount()
        {
            var user = await GetCurrentUser();
            if (user == null)
                return Fail(ECollection.NOT_AUTHORIZED);

            if (user.Status == UserStatus.Blocked)
                return Fail(ECollection.USER_BLOCKED);

            user.State = false;
            await _appUserRepository.RemoveSoft(user.Id);

            return Ok();
        }
    }
}
