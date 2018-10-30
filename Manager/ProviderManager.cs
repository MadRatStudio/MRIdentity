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
    public class ProviderManager : BaseManager
    {
        protected readonly ProviderRepository _providerRepository;
        protected readonly ProviderCategoryRepository _providerCategoryRepository;
        protected readonly ProviderTagRepository _providerTagRepository;
        protected readonly ImageTmpBucket _imageTmpBucket;
        protected readonly ImageOriginBucket _imageOriginBucket;


        public ProviderManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper,
            ProviderRepository providerRepository, ProviderCategoryRepository providerCategoryRepository, ProviderTagRepository providerTagRepository,
            ImageTmpBucket imageTmpBucket, ImageOriginBucket imageOriginBucket) : base(httpContextAccessor, appUserManager, mapper)
        {
            _providerRepository = providerRepository;
            _providerTagRepository = providerTagRepository;
            _providerCategoryRepository = providerCategoryRepository;
            _imageTmpBucket = imageTmpBucket;
            _imageOriginBucket = imageOriginBucket;
        }

        /// <summary>
        /// Create new provider
        /// </summary>
        /// <param name="model">new provider model</param>
        /// <returns></returns>
        public async Task<ApiResponse<IdNameModel>> Create(ProviderUpdateModel model)
        {
            if (model == null)
                return Fail(ECollection.Select(ECollection.MODEL_DAMAGED, new ModelError("model", "Empty")));

            var user = await GetCurrentUser();
            if (!(await _appUserManager.IsInRoleAsync(user, "MANAGER")) && !(await _appUserManager.IsInRoleAsync(user, "ADMIN")))
                return Fail(ECollection.Select(ECollection.ACCESS_DENIED));

            if (string.IsNullOrWhiteSpace(model.Slug))
                return Fail(ECollection.Select(ECollection.MODEL_DAMAGED, new ModelError("slug", "Empty")));

            model.Slug = model.Slug.ToLower();

            if (string.IsNullOrWhiteSpace(model.Category))
                return Fail(ECollection.Select(ECollection.MODEL_DAMAGED, new ModelError("Category", "Category slug is required")));

            if (model.Translations == null || !model.Translations.Any())
                return Fail(ECollection.Select(ECollection.MODEL_DAMAGED, new ModelError("Translations", "required")));

            var isSlugExists = (await _providerRepository.Count(x => x.Slug == model.Slug && x.State)) > 0;
            if (isSlugExists)
                return Fail(ECollection.Select(ECollection.ENTITY_EXISTS));

            var category = await _providerCategoryRepository.GetFirst(x => x.Slug == model.Category);
            if (category == null)
                return Fail(ECollection.Select(ECollection.MODEL_DAMAGED, new ModelError("Category", $"Category with slug {model.Slug} do not exists")));

            var entity = _mapper.Map<Provider>(model);
            entity.Owner = _mapper.Map<ProviderOwner>(user);
            entity.Category = new ProviderProviderCategory
            {
                CategoryId = category.Id,
                Slug = category.Slug
            };
            entity.Tags = new List<ProviderProviderTag>();

            // set avatar
            if(entity.Avatar != null)
            {
                var image = await _imageOriginBucket.MoveFrom(_imageTmpBucket.BucketFullPath, model.Avatar.Key);
                if (image != null && image.IsSuccess)
                {
                    entity.Avatar.Url = image.Url;
                }
            }

            // set background
            if (entity.Background != null)
            {
                var image = await _imageOriginBucket.MoveFrom(_imageTmpBucket.BucketFullPath, model.Background.Key);
                if (image != null && image.IsSuccess)
                {
                    entity.Background.Url = image.Url;
                }
            }

            var result = await _providerRepository.Insert(entity);
            return Ok(new IdNameModel
            {
                Id = result.Id,
                Name = result.Name
            });

        }

        /// <summary>
        /// Get list of providers
        /// </summary>
        /// <param name="skip">Skip count</param>
        /// <param name="limit">Limit</param>
        /// <param name="languageCode">Language code</param>
        /// <param name="q">Search query</param>
        /// <returns></returns>
        public async Task<ApiListResponse<ProviderShortDisplayModel>> Get(int skip, int limit, string languageCode, string q)
        {
            if (skip < 0) skip = 0;
            if (limit < 1) limit = 1;
            if (limit > MAX_LIMIT) limit = MAX_LIMIT;

            if (string.IsNullOrWhiteSpace(languageCode)) languageCode = DEFAULT_LANGUAGE_CODE;

            var list = (await _providerRepository.Get(x => x.State == true, skip, limit, x => x.CreatedTime, true))?.ToList() ?? new List<Provider>();
            var total = await _providerRepository.Count();

            var response = new ApiListResponse<ProviderShortDisplayModel>(skip, limit)
            {
                Data = new List<ProviderShortDisplayModel>(),
                Total = total
            };

            var categoriesToDownload = list.Select(x => x.Category.CategoryId);
            var tagsToDownload = list.SelectMany(x => x.Tags).GroupBy(x => x.TagId).Select(x => x.Key);

            var allCategories = await _providerCategoryRepository.GetAll(categoriesToDownload);
            var allTags = await _providerTagRepository.GetAll(tagsToDownload);


            foreach (var provider in list)
            {
                var converted = _mapper.Map<ProviderShortDisplayModel>(provider);

                // set category
                var category = allCategories.FirstOrDefault(x => x.Id == provider.Category?.CategoryId);
                if (category != null)
                {
                    converted.Category = new ProviderCategoryDisplayModel
                    {
                        Slug = category.Slug
                    };


                    if (category.Translations.Any(z => z.LanguageCode == languageCode))
                    {
                        converted.Category.Name = category.Translations.FirstOrDefault(x => x.LanguageCode == languageCode)?.Name;
                    }
                    else if (category.Translations.Any(z => z.IsDefault))
                    {
                        converted.Category.Name = category.Translations.FirstOrDefault(x => x.IsDefault)?.Name;
                    }
                    else
                    {
                        converted.Category.Name = category.Translations.FirstOrDefault()?.Name;
                    }
                }

                // set tags
                if (provider.Tags != null && provider.Tags.Any())
                {
                    var tags = new List<ProviderTagDisplayModel>();
                    foreach (var tag in provider.Tags)
                    {
                        var convertedTag = _mapper.Map<ProviderTagDisplayModel>(tag);

                        var t = allTags.FirstOrDefault(x => x.Id == tag.TagId);
                        if (t == null) continue;


                        if (t.Translations.Any(x => x.LanguageCode == languageCode))
                        {
                            convertedTag.Name = t.Translations.FirstOrDefault(x => x.LanguageCode == languageCode)?.Name;
                        }
                        else if (t.Translations.Any(z => z.IsDefault))
                        {
                            convertedTag.Name = t.Translations.FirstOrDefault(x => x.IsDefault)?.Name;
                        }
                        else
                        {
                            continue;
                        }

                        tags.Add(convertedTag);
                    }

                    converted.Tags = tags;
                }
                else
                {
                    converted.Tags = new List<ProviderTagDisplayModel>();
                }


                response.Data.Add(converted);
            }

            return response;
        }

        /// <summary>
        /// Delete provider
        /// </summary>
        /// <param name="id">id of provider</param>
        /// <returns></returns>
        public async Task<ApiResponse> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Fail(ECollection.Select(ECollection.MODEL_DAMAGED, new ModelError("Id", "empty")));

            var user = await GetCurrentUser();
            var entity = await _providerRepository.GetFirst(id);

            if (entity == null)
                return Fail(ECollection.ENTITY_NOT_FOUND);

            if (entity.Owner.Id != user.Id)
                return Fail(ECollection.ACCESS_DENIED);

            var result = await _providerRepository.RemoveSoft(id);

            if (result.ModifiedCount == 1) return Ok();
            return Fail(ECollection.UNDEFINED_ERROR);

        }
    }
}
