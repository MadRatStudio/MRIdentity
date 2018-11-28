using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CommonApi.Errors;
using CommonApi.Models;
using CommonApi.Resopnse;
using CommonApi.Response;
using Dal;
using Infrastructure.Entities;
using Infrastructure.Model.Provider;
using Microsoft.AspNetCore.Http;

namespace Manager
{
    /// <summary>
    /// Control 
    /// </summary>
    public class ProviderRoleManager : BaseManager
    {
        protected readonly ProviderRepository _providerRepository;

        public ProviderRoleManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper,
            ProviderRepository providerRepository) : base(httpContextAccessor, appUserManager, mapper)
        {
            _providerRepository = providerRepository;
        }

        public async Task<ApiResponse<List<ProviderRoleDisplayModel>>> GetProviderRoles(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return Fail(ECollection.Select(ECollection.PROPERTY_REQUIRED, new ModelError("slug", "Property slug is required")));
            }

            slug = slug.ToLower();

            if (!await _providerRepository.ExistsWithOwner(slug, (await GetCurrentUser())?.Id))
                return Fail(ECollection.ACCESS_DENIED);

            var result = (await _providerRepository.GetRolesBySlug(slug))?.Select(x => _mapper.Map<ProviderRoleDisplayModel>(x)).ToList() ?? new List<ProviderRoleDisplayModel>();

            return Ok(result);
        }

        public async Task<ApiResponse<ProviderRoleDisplayModel>> CreateProviderRole(string slug, ProviderRoleCreateModel model)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return Fail(ECollection.Select(ECollection.PROPERTY_REQUIRED, new ModelError("slug", "Property slug is required")));
            }

            slug = slug.ToLower();

            if (!await _providerRepository.ExistsWithOwner(slug, (await GetCurrentUser())?.Id))
                return Fail(ECollection.ACCESS_DENIED);

            model.Name = model.Name.ToUpper();
            var entity = _mapper.Map<ProviderRole>(model);

            if (!await _providerRepository.InsertRoleBySlug(slug, entity))
                return Fail(ECollection.UNDEFINED_ERROR);

            return Ok(_mapper.Map<ProviderRoleDisplayModel>(entity));
        }

        public async Task<ApiResponse> RemoveProviderRole(string slug, string name)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return Fail(ECollection.Select(ECollection.PROPERTY_REQUIRED, new ModelError("slug", "Property slug is required")));
            }

            slug = slug.ToLower();

            if (!await _providerRepository.ExistsWithOwner(slug, (await GetCurrentUser())?.Id))
                return Fail(ECollection.ACCESS_DENIED);

            name = name.ToUpper();

            if (!await _providerRepository.RemoveRoleBySlug(slug, name))
                return Fail(ECollection.UNDEFINED_ERROR);

            return Ok();
        }

    }
}
