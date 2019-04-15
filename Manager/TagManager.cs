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
using MRIdentityClient.Exception.Common;
using MRIdentityClient.Models;
using MRIdentityClient.Response;

namespace Manager
{
    public class TagManager : BaseManager
    {
        protected readonly ProviderTagRepository _providerTagRepository;

        public TagManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper, ILoggerFactory loggerFactory,
            ProviderTagRepository providerTagRepository) : base(httpContextAccessor, appUserManager, mapper, loggerFactory)
        {
            _providerTagRepository = providerTagRepository;
        }

        public async Task<IdNameModel> Create(ProviderTagCreateModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Key))
                throw new ModelDamagedException("Key is null");

            if (model.Translations == null || !model.Translations.Any())
                throw new ModelDamagedException("Key is null");

            if (model.Translations.Count(z => z.IsDefault) != 1)
                throw new ModelDamagedException("No default translation");

            model.Key = model.Key.ToLower();
            var exists = await _providerTagRepository.Count(x => x.Key == model.Key && x.State);

            if (exists > 0)
                throw new EntityExistsException("1", "1", typeof(ProviderTag));

            ProviderTag tag = new ProviderTag
            {
                Key = model.Key,
                UserCreatedId = "TEST_USER_ID",
                State = true,
                Translations = model.Translations.Select(x => new ProviderTagTranslation
                {
                    IsDefault = x.IsDefault,
                    LanguageCode = x.LanguageCode,
                    Name = x.Name
                }).ToList()
            };

            await _providerTagRepository.Insert(tag);
            return new IdNameModel
            {
                Id = tag.Id,
                Name = tag.Translations.First(x => x.IsDefault).Name
            };
        }

        public async Task<ApiListResponse<ProviderTagDisplayModel>> Get(int skip, int limit, string languageCode, string q)
        {
            if (string.IsNullOrWhiteSpace(languageCode)) languageCode = DEFAULT_LANGUAGE_CODE;
            else languageCode = languageCode.ToLower();

            if (skip < 0) skip = 0;
            if (limit < 1) limit = 1;
            if (limit > MAX_LIMIT) limit = MAX_LIMIT;

            var result = new ApiListResponse<ProviderTagDisplayModel>(skip, limit)
            {
                Data = new List<ProviderTagDisplayModel>(),
                Total = await _providerTagRepository.Count()
            };

            var tags = (await _providerTagRepository.Search(skip, limit, q))?.ToList() ?? new List<Infrastructure.Entities.ProviderTag>();

            foreach (var tag in tags)
            {
                var name = string.Empty;
                if (tag.Translations.Any(z => z.LanguageCode == languageCode))
                {
                    name = tag.Translations.FirstOrDefault(x => x.LanguageCode == languageCode).Name;
                }
                else if (tag.Translations.Any(z => z.IsDefault))
                {
                    name = tag.Translations.FirstOrDefault(x => x.IsDefault).Name;
                }
                else
                {
                    continue;
                }

                result.Data.Add(new ProviderTagDisplayModel
                {
                    Key = tag.Key,
                    Name = name
                });
            }

            return result;
        }
    }
}
