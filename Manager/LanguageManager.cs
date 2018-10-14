using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CommonApi.Response;
using Dal;
using Infrastructure.Entities;
using Infrastructure.Model.Common;
using Microsoft.AspNetCore.Http;
using MRDbIdentity.Infrastructure.Interface;

namespace Manager
{
    public class LanguageManager : BaseManager
    {
        protected readonly LanguageRepository _languageRepository;

        public LanguageManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper,
            LanguageRepository languageRepository) : base(httpContextAccessor, appUserManager, mapper)
        {
            _languageRepository = languageRepository;
        }

        public async Task<ApiListResponse<LanguageDisplayModel>> Search(int skip, int limit, string q)
        {
            var result = new ApiListResponse<LanguageDisplayModel>(skip, limit);

            var list = await _languageRepository.Search(skip, limit, q);
            var total = await _languageRepository.Count();

            result.Data = list?.Select(x => _mapper.Map<LanguageDisplayModel>(x)).ToList();
            result.Total = total;

            return result;
        }
    }
}
