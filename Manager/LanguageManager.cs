using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dal;
using Infrastructure.Model.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MRIdentityClient.Response;

namespace Manager
{
    public class LanguageManager : BaseManager
    {
        protected readonly LanguageRepository _languageRepository;

        public LanguageManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper, ILoggerFactory loggerFactory,
            LanguageRepository languageRepository) : base(httpContextAccessor, appUserManager, mapper, loggerFactory)
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

        public async Task<ApiListResponse<LanguageDisplayModel>> All()
        {
            var total = await _languageRepository.Count();
            var list = await _languageRepository.Search(0, (int)total, string.Empty);

            return new ApiListResponse<LanguageDisplayModel>(_mapper.Map<List<LanguageDisplayModel>>(list), 0, (int)total, total);
        }
    }
}
