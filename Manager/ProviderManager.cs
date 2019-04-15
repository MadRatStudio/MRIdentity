using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dal;
using Infrastructure.Entities;
using Infrastructure.Model.Provider;
using Manager.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MRIdentityClient.Exception.Common;
using MRIdentityClient.Exception.MRSystem;
using MRIdentityClient.Exception.Request;
using MRIdentityClient.Models;
using MRIdentityClient.Response;
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
        public async Task<IdNameModel> Create(ProviderUpdateModel model)
        {
            if (model == null)
                throw new ModelDamagedException(nameof(model), "can not be empty");

            var user = await GetCurrentUser();
            if (!(await _appUserManager.IsInRoleAsync(user, "MANAGER")) && !(await _appUserManager.IsInRoleAsync(user, "ADMIN")))
                throw new NotEnoughRightsException();

            if (string.IsNullOrWhiteSpace(model.Slug))
                throw new ModelDamagedException(nameof(model.Slug), "can not be empty");

            model.Slug = model.Slug.ToLower();

            if (string.IsNullOrWhiteSpace(model.Category))
                throw new ModelDamagedException(nameof(model.Category), "can not be empty");

            if (model.Translations == null || !model.Translations.Any())
                throw new ModelDamagedException(nameof(model.Translations), "can not be null or empty");

            var isSlugExists = (await _providerRepository.Count(x => x.Slug == model.Slug && x.State)) > 0;
            if (isSlugExists)
                throw new MRSystemException($"Provider with slug {model.Slug} already exists");

            var category = await _providerCategoryRepository.GetFirst(x => x.Slug == model.Category);
            if (category == null)
                throw new EntityNotFoundException(model.Category, typeof(ProviderCategory));

            var entity = _mapper.Map<Provider>(model);
            entity.Owner = _mapper.Map<ProviderOwner>(user);
            entity.Category = new ProviderProviderCategory
            {
                CategoryId = category.Id,
                Slug = category.Slug
            };
            entity.Tags = new List<ProviderProviderTag>();

            entity.Workers = new List<ProviderWorker>
            {
                new ProviderWorker
                {
                    Roles = new List<ProviderWorkerRole>
                    {
                        ProviderWorkerRole.ANALYTICS,
                        ProviderWorkerRole.DEVELOPER,
                        ProviderWorkerRole.MANAGER,
                        ProviderWorkerRole.OWNER,
                        ProviderWorkerRole.USER_MANAGER
                    },
                    UserEmail = _currentUserEmail,
                    UserId = _currentUserId
                }
            };

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
            return new IdNameModel
            {
                Id = result.Id,
                Name = result.Name
            };

        }

        /// <summary>
        /// Create fingerprint for provider
        /// </summary>
        /// <param name="providerId">Target provider id</param>
        /// <param name="model">Create fingerprint model</param>
        /// <returns>Provider fingerprint display model</returns>
        public async Task<ProviderFingerprintDisplayModel> CreateFingerprint(string providerId, ProviderFingerprintCreateModel model)
        {
            if (string.IsNullOrWhiteSpace(providerId))
                throw new ModelDamagedException(nameof(providerId), "is required");

            if (model == null)
                throw new ModelDamagedException(nameof(model), "is required");

            if (string.IsNullOrWhiteSpace(model.Name))
                throw new ModelDamagedException(nameof(model.Name), "is required");

            var entity = await _providerRepository.GetFirst(providerId);
            if (entity == null)
                throw new EntityNotFoundException(providerId, typeof(Provider));

            if (entity.Owner.Id != (await GetCurrentUser())?.Id)
                throw new AccessDeniedException(entity.Id, typeof(Provider));

            if (entity.Fingerprints == null)
                entity.Fingerprints = new List<ProviderFingerprint>();

            if (entity.Fingerprints.Any(x => x.Name.ToLower() == model.Name.ToLower()))
                throw new MRSystemException($"Fingerprint with name {model.Name} already exists");

            if (entity.Fingerprints == null)
                entity.Fingerprints = new List<ProviderFingerprint>();

            var fingerprint = _mapper.Map<ProviderFingerprint>(model);

            fingerprint.Fingerprint = FingerprintGenerator.Generate();
            fingerprint.FingerprintUpdateTime = DateTime.UtcNow;

            entity.Fingerprints.Add(fingerprint);
            await _providerRepository.UpdateFingerprints(entity);

            return _mapper.Map<ProviderFingerprintDisplayModel>(fingerprint);
        }

        /// <summary>
        /// Get provider
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<ProviderDisplayModel> GetToDisplay(string slug, string languageCode)
        {
            var entity = await _providerRepository.GetFirst(x => x.Slug == slug && x.State);
            if (entity == null)
                throw new EntityNotFoundException(slug, typeof(Provider));

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

            return model;
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
                throw new BadRequestException();

            var user = await GetCurrentUser();
            if (!await _providerRepository.ExistsWithOwner(id, user.Id))
                throw new AccessDeniedException(id, typeof(Provider));

            var entity = await _providerRepository.GetFirst(id);
            if (entity.Fingerprints == null || !entity.Fingerprints.Any())
                return new ApiListResponse<ProviderFingerprintDisplayModel>
                {
                    Data = new List<ProviderFingerprintDisplayModel>(),
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
        public async Task<ProviderUpdateModel> GetUpdateModel(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new BadRequestException();

            var entity = await _providerRepository.GetFirst(x => x.Slug == slug.ToLower() && x.State);
            if (entity == null)
                throw new EntityNotFoundException(slug, typeof(Provider));

            var user = await GetCurrentUser();
            if (entity.Owner.Id != user.Id && !_currentUserIsAdmin)
                throw new AccessDeniedException(slug, typeof(Provider));

            var model = _mapper.Map<ProviderUpdateModel>(entity);
            model.Category = entity.Category.Slug;

            return model;
        }

        /// <summary>
        /// Updates provider model
        /// </summary>
        /// <param name="model">Provider to update</param>
        /// <returns>Provider updpate model</returns>
        public async Task<ProviderUpdateModel> Update(ProviderUpdateModel model)
        {
            if (model == null)
                throw new BadRequestException();

            var user = await GetCurrentUser();
            if (!(await _appUserManager.IsInRoleAsync(user, "MANAGER")) && !(await _appUserManager.IsInRoleAsync(user, "ADMIN")))
                throw new NotEnoughRightsException();

            if (string.IsNullOrWhiteSpace(model.Slug))
                throw new ModelDamagedException(nameof(model.Slug), "can not be null");

            model.Slug = model.Slug.ToLower();

            if (string.IsNullOrWhiteSpace(model.Category))
                throw new ModelDamagedException(nameof(model.Category), "can not be null");

            if (model.Translations == null || !model.Translations.Any())
                throw new ModelDamagedException(nameof(model.Translations), "can not be null or empty");

            if (model.Translations.Count(x => x.IsDefault) != 1)
                throw new ModelDamagedException(nameof(model.Translations), "default is required");

            var entity = await _providerRepository.GetFirst(model.Id);
            if (entity == null)
                throw new EntityNotFoundException(model.Id, typeof(Provider));

            if (entity.Owner.Id != (await GetCurrentUser()).Id)
                throw new AccessDeniedException(model.Id, typeof(Provider));

            var category = await _providerCategoryRepository.GetFirst(x => x.Slug == model.Category);
            if (category == null)
                throw new EntityNotFoundException(model.Category, typeof(ProviderCategory));

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
                    throw new MRSystemException("Can not transfer image");

                newEntity.Avatar.Url = aMoveResponse.Url;
                removeAvatar = true;
            }

            if (newEntity.Background.Key != entity.Background.Key)
            {
                var bMoveResponse = await _imageOriginBucket.MoveFrom(_imageTmpBucket.BucketFullPath, newEntity.Avatar.Key);
                if (!bMoveResponse.IsSuccess)
                    throw new MRSystemException("Can not transfer image");

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

            if (newEntity.Workers == null)
                newEntity.Workers = new List<ProviderWorker>();

            var replaceResponse = await _providerRepository.Replace(newEntity);
            if (replaceResponse == null)
                throw new MRSystemException();

            return _mapper.Map<ProviderUpdateModel>(newEntity);
        }

        /// <summary>
        /// Delete provider
        /// </summary>
        /// <param name="id">id of provider</param>
        public async Task Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ModelDamagedException(nameof(id), "can not be empty");

            var user = await GetCurrentUser();
            var entity = await _providerRepository.GetFirst(id);

            if (entity == null)
                throw new EntityNotFoundException(id, typeof(Provider));

            if (entity.Owner.Id != user.Id)
                throw new AccessDeniedException(id, typeof(Provider));

            var result = await _providerRepository.RemoveSoft(id);

            if (result.ModifiedCount != 1)
                throw new MRSystemException();

        }

        /// <summary>
        /// Delete providers fingerprint
        /// </summary>
        /// <param name="id">provider id</param>
        /// <param name="name">fingerprint name</param>
        public async Task DeleteFingerprint(string id, string name)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                throw new ModelDamagedException($"{nameof(id)} and {nameof(name)}", "are required");

            var user = await GetCurrentUser();
            if (!(await _providerRepository.ExistsWithOwner(id, user.Id)))
                throw new EntityNotFoundException(id, typeof(Provider));

            var entity = await _providerRepository.GetFirst(id);
            if (entity.Fingerprints == null || !entity.Fingerprints.Any(x => x.Name.ToLower() == name.ToLower()))
                throw new EntityNotFoundException("name", typeof(ProviderFingerprint));

            entity.Fingerprints.RemoveAll(x => x.Name.ToLower() == name.ToLower());
            await _providerRepository.Replace(entity);
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
