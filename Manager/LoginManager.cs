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
using Infrastructure.Model.User;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;

namespace Manager
{
    public class LoginManager : BaseManager
    {
        protected readonly AppUserRepository _appUserRepository;
        protected readonly ProviderRepository _providerRepository;

        public LoginManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper,
            AppUserRepository appUserRepository, ProviderRepository providerRepository) : base(httpContextAccessor, appUserManager, mapper)
        {
            _appUserRepository = appUserRepository;
            _providerRepository = providerRepository;
        }

        public async Task<ApiResponse> ProviderLoginEmail(HttpContext context, UserProviderEmailLogin model)
        {
            var entity = await _appUserManager.FindByEmailAsync(model.Email);
            if (entity == null || !(await _appUserManager.CheckPasswordAsync(entity, model.Password))) return Fail(ECollection.USER_NOT_FOUND);

            var provider = await _providerRepository.GetFirst(x => x.Id == model.ProviderId && x.State);
            if (provider == null)
                return Fail(ECollection.PROVIDER_NOT_FOUND);

            if (!provider.IsLoginEnabled)
                return Fail(ECollection.PROVIDER_UNAVALIABLE);

            UpdateResult updateResult;

            var connectedProvider = await _appUserRepository.GetProvider(entity.Id, model.ProviderId);
            if (connectedProvider != null)
            {
                updateResult = await _appUserRepository.AddProviderMeta(entity.Id, provider.Name, _createMeta(context));
            }
            else
            {
                connectedProvider = new AppUserProvider
                {
                    Metadata = new List<AppUserProviderMeta>() { _createMeta(context) },
                    ProviderId = provider.Id,
                    ProviderName = provider.Name,
                    UpdatedTime = DateTime.UtcNow
                };

                updateResult = await _appUserRepository.AddProvider(entity.Id, connectedProvider);
            }

            if (updateResult.ModifiedCount == 1)
                return Ok();

            return Fail(ECollection.UNDEFINED_ERROR);
        }

        public async Task<ApiResponse> ProviderLoginInstant(HttpContext context, string providerId)
        {
            var user = await GetCurrentUser();
            if (user == null)
                return Fail(ECollection.NOT_AUTHORIZED);

            var provider = await _providerRepository.GetFirst(x => x.Id == providerId && x.State);
            if (provider == null)
                return Fail(ECollection.PROVIDER_NOT_FOUND);

            if (!provider.IsLoginEnabled)
                return Fail(ECollection.PROVIDER_UNAVALIABLE);

            UpdateResult updateResult = null;

            var connectedProvider = await _appUserRepository.GetProvider(user.Id, provider.Id);
            if (connectedProvider != null)
            {
                updateResult = await _appUserRepository.AddProviderMeta(user.Id, provider.Id, _createMeta(context));
            }
            else
            {
                connectedProvider = new AppUserProvider
                {
                    Metadata = new List<AppUserProviderMeta>() { _createMeta(context) },
                    ProviderId = provider.Id,
                    ProviderName = provider.Name,
                    UpdatedTime = DateTime.UtcNow
                };

                updateResult = await _appUserRepository.AddProvider(user.Id, connectedProvider);
            }

            if (updateResult.ModifiedCount == 1)
                return Ok();

            return Fail(ECollection.UNDEFINED_ERROR);
        }

        protected AppUserProviderMeta _createMeta(HttpContext context)
        {
            var userAgent = "Unknown agent";
            Microsoft.Extensions.Primitives.StringValues agent = new Microsoft.Extensions.Primitives.StringValues();
            if (context.Request.Headers.TryGetValue("User-Agent", out agent))
            {
                userAgent = agent.FirstOrDefault() ?? "Unknown agent";
            }

            return new AppUserProviderMeta
            {
                Ip = context.Connection.RemoteIpAddress.ToString(),
                UserAgent = userAgent,
                UpdatedTime = DateTime.UtcNow
            };
        }
    }
}
