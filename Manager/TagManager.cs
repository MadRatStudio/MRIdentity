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
using Microsoft.Extensions.Logging;

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

        public async Task<ApiResponse<IdNameModel>> Create(ProviderTagCreateModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Key)) return Fail(ECollection.Select(ECollection.MODEL_DAMAGED, new ModelError
            {
                Error = "Key can not be empty",
                Property = "Key"
            }));

            if (model.Translations == null || !model.Translations.Any()) return Fail(ECollection.Select(ECollection.MODEL_DAMAGED, new ModelError
            {
                Error = "Translations list can not be empty",
                Property = "Translations"
            }));

            if (model.Translations.Count(z => z.IsDefault) != 1) return Fail(ECollection.Select(ECollection.MODEL_DAMAGED, new ModelError
            {
                Error = "One translation must be default",
                Property = "Translations"
            }));

            model.Key = model.Key.ToLower();
            var exists = await _providerTagRepository.Count(x => x.Key == model.Key && x.State);

            if (exists > 0) return Fail(ECollection.Select(ECollection.ENTITY_EXISTS, new ModelError
            {
                Error = $"Tag with key {model.Key} already exists",
                Property = "Key"
            }));

            ProviderTag tag = new ProviderTag
            {
                Key = model.Key,
                //UserCreatedId = (await GetCurrentUser()).Id,
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
            return Ok(new IdNameModel
            {
                Id = tag.Id,
                Name = tag.Translations.First(x => x.IsDefault).Name
            });
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

            foreach(var tag in tags)
            {
                var name = string.Empty;
                if(tag.Translations.Any(z => z.LanguageCode == languageCode))
                {
                    name = tag.Translations.FirstOrDefault(x => x.LanguageCode == languageCode).Name;
                }
                else if(tag.Translations.Any(z => z.IsDefault))
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
