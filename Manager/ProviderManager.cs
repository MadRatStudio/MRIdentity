using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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
using Manager.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Tools;

namespace Manager
{
    public class ProviderManager : BaseManager
    {
        protected readonly ProviderRepository _providerRepository;
        protected readonly ProviderCategoryRepository _providerCategoryRepository;
        protected readonly ProviderTagRepository _providerTagRepository;
        protected readonly ImageTmpBucket _imageTmpBucket;
        protected readonly ImageOriginBucket _imageOriginBucket;


        public ProviderManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper, ILoggerFactory loggerFactory,
            ProviderRepository providerRepository, ProviderCategoryRepository providerCategoryRepository, ProviderTagRepository providerTagRepository,
            ImageTmpBucket imageTmpBucket, ImageOriginBucket imageOriginBucket) : base(httpContextAccessor, appUserManager, mapper, loggerFactory)
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
            if (entity.Avatar != null)
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
        /// Create fingerprint for provider
        /// </summary>
        /// <param name="providerId">Target provider id</param>
        /// <param name="model">Create fingerprint model</param>
        /// <returns>Provider fingerprint display model</returns>
        public async Task<ApiResponse<ProviderFingerprintDisplayModel>> CreateFingerprint(string providerId, ProviderFingerprintCreateModel model)
        {
            if (string.IsNullOrWhiteSpace(providerId))
                return Fail(ECollection.MODEL_DAMAGED);

            if (model == null)
                return Fail(ECollection.MODEL_DAMAGED);

            if (string.IsNullOrWhiteSpace(model.Name))
                return Fail(ECollection.Select(ECollection.MODEL_DAMAGED, new ModelError("Name", "Name is required")));

            var entity = await _providerRepository.GetFirst(providerId);
            if (entity == null)
                return Fail(ECollection.ENTITY_NOT_FOUND);

            if (entity.Owner.Id != (await GetCurrentUser())?.Id)
                return Fail(ECollection.ACCESS_DENIED);

            if (entity.Fingerprints == null)
                entity.Fingerprints = new List<ProviderFingerprint>();

            if (entity.Fingerprints.Any(x => x.Name.ToLower() == model.Name.ToLower()))
                return Fail(ECollection.Select(ECollection.ENTITY_EXISTS, new ModelError("Fingerprint", "Fingerprint with this name is exists")));

            if (entity.Fingerprints == null)
                entity.Fingerprints = new List<ProviderFingerprint>();


            var fingerprint = _mapper.Map<ProviderFingerprint>(model);

            fingerprint.Fingerprint = FingerprintGenerator.Generate();
            fingerprint.FingerprintUpdateTime = DateTime.UtcNow;

            entity.Fingerprints.Add(fingerprint);
            await _providerRepository.UpdateFingerprints(entity);

            return Ok(_mapper.Map<ProviderFingerprintDisplayModel>(fingerprint));
        }

        /// <summary>
        /// Get provider
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<ApiResponse<ProviderDisplayModel>> GetToDisplay(string slug, string languageCode)
        {
            var entity = await _providerRepository.GetFirst(x => x.Slug == slug && x.State);
            if (entity == null)
                return Fail(ECollection.ENTITY_NOT_FOUND);

            var model = _mapper.Map<ProviderDisplayModel>(entity);
            if (string.IsNullOrWhiteSpace(languageCode))
                languageCode = DEFAULT_LANGUAGE_CODE;

            var translation = entity.Translations?.FirstOrDefault(x => x.LanguageCode == languageCode);
            if (translation == null)
            {
                translation = entity.Translations?.FirstOrDefault(x => x.LanguageCode == DEFAULT_LANGUAGE_CODE);
            }

            if (translation == null)
            {
                translation = new ProviderTranslation();
            }

            model.Description = translation.Description;
            model.DisplayName = translation.DisplayName;
            model.KeyWords = translation.KeyWords;

            return Ok(model);
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
            var tagsToDownload = list.Where(x => x.Tags != null).SelectMany(x => x.Tags).GroupBy(x => x.TagId).Select(x => x.Key);

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
        /// Get provider`s fingerprints
        /// </summary>
        /// <param name="id">Id of provider</param>
        /// <returns>List response of provider`s fingerprints</returns>
        public async Task<ApiListResponse<ProviderFingerprintDisplayModel>> GetProviderFingerprints(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return FailList<ProviderFingerprintDisplayModel>(ECollection.MODEL_DAMAGED);

            var user = await GetCurrentUser();
            if (!await _providerRepository.ExistsWithOwner(id, user.Id))
                return FailList<ProviderFingerprintDisplayModel>(ECollection.ACCESS_DENIED);

            var entity = await _providerRepository.GetFirst(id);
            if (entity.Fingerprints == null || !entity.Fingerprints.Any())
                return new ApiListResponse<ProviderFingerprintDisplayModel>
                {
                    Data = new List<ProviderFingerprintDisplayModel>(),
                    Error = null,
                    Skip = 0,
                    Limit = 1,
                    Total = 0
                };

            var list = entity.Fingerprints.Select(x => _mapper.Map<ProviderFingerprintDisplayModel>(x)).ToList();
            return new ApiListResponse<ProviderFingerprintDisplayModel>
            {
                Data = list,
                Limit = list.Count,
                Skip = 0,
                Total = list.Count
            };
        }

        /// <summary>
        /// Get model for provider update
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<ApiResponse<ProviderUpdateModel>> GetUpdateModel(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return Fail(ECollection.MODEL_DAMAGED);

            var entity = await _providerRepository.GetFirst(x => x.Slug == slug.ToLower() && x.State);
            if (entity == null)
                return Fail(ECollection.ENTITY_NOT_FOUND);

            var user = await GetCurrentUser();
            if (entity.Owner.Id != user.Id)
                return Fail(ECollection.ACCESS_DENIED);

            var model = _mapper.Map<ProviderUpdateModel>(entity);
            model.Category = entity.Category.Slug;

            return Ok(model);
        }

        /// <summary>
        /// Updates provider model
        /// </summary>
        /// <param name="model">Provider to update</param>
        /// <returns>Provider updpate model</returns>
        public async Task<ApiResponse<ProviderUpdateModel>> Update(ProviderUpdateModel model)
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

            if (model.Translations.Count(x => x.IsDefault) != 1)
                return Fail(ECollection.Select(ECollection.MODEL_DAMAGED, new ModelError("Translation", "Required one default translation")));

            var entity = await _providerRepository.GetFirst(model.Id);
            if (entity == null)
                return Fail(ECollection.Select(ECollection.USER_NOT_FOUND));

            if (entity.Owner.Id != (await GetCurrentUser()).Id)
                return Fail(ECollection.ACCESS_DENIED);

            var category = await _providerCategoryRepository.GetFirst(x => x.Slug == model.Category);
            if (category == null)
                return Fail(ECollection.Select(ECollection.ENTITY_NOT_FOUND, new ModelError("Category", "Category not found")));

            var newEntity = _mapper.Map<Provider>(model);
            newEntity.Owner = entity.Owner;
            newEntity.Category = new ProviderProviderCategory
            {
                CategoryId = category.Id,
                Slug = category.Slug
            };

            bool removeAvatar = false;
            bool removeBackground = false;

            if (newEntity.Avatar.Key != entity.Avatar.Key)
            {
                var aMoveResponse = await _imageOriginBucket.MoveFrom(_imageTmpBucket.BucketFullPath, newEntity.Avatar.Key);
                if (!aMoveResponse.IsSuccess)
                    return Fail(ECollection.TRANSFER_IMAGE_ERROR);

                newEntity.Avatar.Url = aMoveResponse.Url;
                removeAvatar = true;
            }

            if (newEntity.Background.Key != entity.Background.Key)
            {
                var bMoveResponse = await _imageOriginBucket.MoveFrom(_imageTmpBucket.BucketFullPath, newEntity.Avatar.Key);
                if (!bMoveResponse.IsSuccess)
                    return Fail(ECollection.TRANSFER_IMAGE_ERROR);

                newEntity.Background.Key = bMoveResponse.Key;
                removeBackground = true;
            }

            if (removeAvatar)
            {
                await _imageOriginBucket.Delete(entity.Avatar.Key);
                await _imageTmpBucket.Delete(newEntity.Avatar.Key);
            }

            if (removeBackground)
            {
                await _imageOriginBucket.Delete(entity.Background.Key);
                await _imageTmpBucket.Delete(newEntity.Background.Key);
            }

            var replaceResponse = await _providerRepository.Replace(newEntity);
            if (replaceResponse == null)
                return Fail(ECollection.UNDEFINED_ERROR);

            return Ok(_mapper.Map<ProviderUpdateModel>(newEntity));
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

        /// <summary>
        /// Delete providers fingerprint
        /// </summary>
        /// <param name="id">provider id</param>
        /// <param name="name">fingerprint name</param>
        /// <returns></returns>
        public async Task<ApiResponse> DeleteFingerprint(string id, string name)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                return Fail(ECollection.MODEL_DAMAGED);

            var user = await GetCurrentUser();
            if (!(await _providerRepository.ExistsWithOwner(id, user.Id)))
                return Fail(ECollection.ENTITY_NOT_FOUND);

            var entity = await _providerRepository.GetFirst(id);
            if (entity.Fingerprints == null || !entity.Fingerprints.Any(x => x.Name.ToLower() == name.ToLower()))
                return Fail(ECollection.Select(ECollection.ENTITY_NOT_FOUND, new ModelError("Fingerprint", "Fingerprint not found")));

            entity.Fingerprints.RemoveAll(x => x.Name.ToLower() == name.ToLower());
            await _providerRepository.Replace(entity);

            return Ok();
        }

        protected string _generateFingerprint(Provider provider, ProviderFingerprint fingerprint)
        {
            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: fingerprint.Domain,
                notBefore: now,
                claims: _generateClaims(provider, fingerprint).Claims,
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        protected ClaimsIdentity _generateClaims(Provider provider, ProviderFingerprint fingerprint)
        {
            var list = new List<Claim>
            {
                new Claim(ProviderTokenOptions.PROVIDER_ID_NAME, provider.Id),
                new Claim(ProviderTokenOptions.PROVIDER_OWNER_ID_NAME, provider.Owner.Id),
                new Claim(ProviderTokenOptions.PROVIDER_DOMAIN_NAME, fingerprint.Domain),
            };

            return new ClaimsIdentity(list);
        }
    }
}
