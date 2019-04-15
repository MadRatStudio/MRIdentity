using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dal;
using Infrastructure.Entities;
using Infrastructure.Model.Provider;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MRIdentityClient.Exception.Basic;
using MRIdentityClient.Exception.Common;
using MRIdentityClient.Response;

namespace Manager
{
    /// <summary>
    /// Control 
    /// </summary>
    public class ProviderRoleManager : BaseManager
    {
        protected readonly ProviderRepository _providerRepository;

        public ProviderRoleManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper, ILoggerFactory loggerFactory,
            ProviderRepository providerRepository) : base(httpContextAccessor, appUserManager, mapper, loggerFactory)
        {
            _providerRepository = providerRepository;
        }

        public async Task<ApiListResponse<ProviderRoleDisplayModel>> GetProviderRoles(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ModelDamagedException("slug is null");

            slug = slug.ToLower();

            if (!await _providerRepository.ExistsWithOwnerSlug(slug, (await GetCurrentUser())?.Id))
                throw new AccessDeniedException(slug, typeof(Provider));

            var result = (await _providerRepository.GetRolesBySlug(slug))?.Select(x => _mapper.Map<ProviderRoleDisplayModel>(x)).ToList() ?? new List<ProviderRoleDisplayModel>();

            return new ApiListResponse<ProviderRoleDisplayModel>(result, 0, 0, result.Count);
        }

        public async Task<ProviderRoleDisplayModel> CreateProviderRole(string slug, ProviderRoleCreateModel model)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ModelDamagedException("Slug is required");

            slug = slug.ToLower();

            if (!await _providerRepository.ExistsWithOwnerSlug(slug, (await GetCurrentUser())?.Id))
                throw new AccessDeniedException(slug, typeof(Provider));

            model.Name = model.Name.ToUpper();
            var entity = _mapper.Map<ProviderRole>(model);

            if (!await _providerRepository.InsertRoleBySlug(slug, entity))
                throw new MRException();

            return _mapper.Map<ProviderRoleDisplayModel>(entity);
        }

        public async Task<ApiOkResult> RemoveProviderRole(string slug, string name)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ModelDamagedException("Slug is required");

            slug = slug.ToLower();

            if (!await _providerRepository.ExistsWithOwnerSlug(slug, (await GetCurrentUser())?.Id))
                throw new AccessDeniedException(slug, typeof(Provider));

            name = name.ToUpper();

            if (!await _providerRepository.RemoveRoleBySlug(slug, name))
                throw new MRException();

            return new ApiOkResult();
        }

    }
}
