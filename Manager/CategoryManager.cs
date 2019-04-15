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
    public class CategoryManager : BaseManager
    {
        protected readonly ProviderCategoryRepository _providerCategoryRepository;
        protected readonly ProviderRepository _providerRepository;

        public CategoryManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper, ILoggerFactory loggerFactory,
            ProviderCategoryRepository providerCategoryRepository, ProviderRepository providerRepository) : base(httpContextAccessor, appUserManager, mapper, loggerFactory)
        {
            _providerCategoryRepository = providerCategoryRepository;
            _providerRepository = providerRepository;
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="model">Model to insert in db</param>
        /// <returns>ApiResponse IdNameModel / ApiError</returns>
        public async Task<IdNameModel> Create(CategoryUpdateModel model)
        {
            CheckUpdateModel(model);

            model.Slug = model.Slug.ToLower();
            if ((await _providerCategoryRepository.Count(x => x.Slug == model.Slug && x.State)) > 0)
                throw new ModelDamagedException(nameof(model.Slug), "category with this slug already exists");

            var entity = _mapper.Map<ProviderCategory>(model);
            await _providerCategoryRepository.Insert(entity);

            return new IdNameModel
            {
                Id = entity.Id,
                Name = entity.Slug
            };
        }

        /// <summary>
        /// Get list
        /// </summary>
        /// <param name="skip">Skip count</param>
        /// <param name="limit">Limit count</param>
        /// <param name="q">Search query</param>
        /// <param name="languageCode">Language code</param>
        /// <returns>Result list</returns>
        public async Task<ApiListResponse<ProviderCategoryDisplayModel>> Get(int skip, int limit, string q, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode)) languageCode = DEFAULT_LANGUAGE_CODE;
            else languageCode = languageCode.ToLower();

            if (!string.IsNullOrWhiteSpace(q)) q = q.ToLower();

            if (skip < 0) skip = 0;
            if (limit < 1) limit = 1;
            else if (limit > MAX_LIMIT) limit = MAX_LIMIT;

            var list = (await _providerCategoryRepository.Search(skip, limit, q))?.ToList() ?? new List<Infrastructure.Entities.ProviderCategory>();

            var response = new ApiListResponse<ProviderCategoryDisplayModel>(skip, limit)
            {
                Data = new List<ProviderCategoryDisplayModel>(),
                Total = await _providerCategoryRepository.Count()
            };

            foreach (var cat in list)
            {
                ProviderCategoryDisplayModel model = new ProviderCategoryDisplayModel()
                {
                    Slug = cat.Slug,
                };

                ProviderCategoryTranslation translation = null;

                if (cat.Translations.Any(x => x.LanguageCode == languageCode))
                {

                    translation = cat.Translations.First(x => x.LanguageCode == languageCode);
                }
                else
                {
                    translation = cat.Translations.FirstOrDefault(x => x.IsDefault);
                }

                if (translation == null) continue;
                model.Name = translation.Name;
                model.Description = translation.Description;

                response.Data.Add(model);
            }

            return response;
        }

        /// <summary>
        /// Get category model to update
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        /// TODO Add access check
        public async Task<CategoryUpdateModel> Get(string slug)
        {
            var entity = await _providerCategoryRepository.GetFirst(x => x.Slug == slug);
            if (entity == null)
                throw new EntityNotFoundException(slug, typeof(ProviderCategory), "Can not find category");

            var model = new CategoryUpdateModel
            {
                Slug = entity.Slug,
                Translations = entity.Translations.Select(x =>
                    new CategoryTranslationUpdateModel
                    {
                        Description = x.Description,
                        IsDefault = x.IsDefault,
                        LanguageCode = x.LanguageCode,
                        Name = x.Name
                    }).ToList()
            };

            return model;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IdNameModel> Update(string id, CategoryUpdateModel model)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ModelDamagedException(nameof(id), "is required");

            var existCount = await _providerCategoryRepository.Count(x => x.Id == id);
            if (existCount != 1)
                throw new EntityNotFoundException(id, typeof(ProviderCategory));

            CheckUpdateModel(model);

            var entity = _mapper.Map<ProviderCategory>(model);
            await _providerCategoryRepository.Replace(entity);

            return new IdNameModel(entity.Id, entity.Slug);
        }

        /// <summary>
        /// Delete soft
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ModelDamagedException(nameof(id), "is required");

            var count = await _providerCategoryRepository.Count(x => x.State && x.Id == id);
            if (count < 1)
                throw new EntityNotFoundException(id, typeof(ProviderCategory));

            await _providerCategoryRepository.RemoveSoft(id);
        }

        protected void CheckUpdateModel(CategoryUpdateModel model)
        {
            if (model == null)
                throw new ModelDamagedException(nameof(model), "is required");

            if (string.IsNullOrWhiteSpace(model.Slug))
                throw new ModelDamagedException(nameof(model.Slug), "is required");

            if (model.Translations == null || !model.Translations.Any())
                throw new ModelDamagedException(nameof(model.Translations), "is required");

            if (model.Translations.Count(x => x.IsDefault) != 1)
                throw new ModelDamagedException(nameof(model.Translations), "one must be default");

            var damagedTranslation = model.Translations.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Name) || string.IsNullOrWhiteSpace(x.LanguageCode));
            if (damagedTranslation != null)
                throw new ModelDamagedException(nameof(model.Translations), "damaged");

            if (model.Translations.GroupBy(x => x.LanguageCode).Any(x => x.Count() > 1))
                throw new ModelDamagedException(nameof(model.Translations), "one must be default");
        }
    }
}
