using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dal;
using Infrastructure.Entities;
using Infrastructure.Model.Provider;
using Infrastructure.System.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MRIdentityClient.Exception.Basic;
using MRIdentityClient.Exception.Common;
using MRIdentityClient.Response;
using Tools;

namespace Manager
{
    public class ProviderWorkerManager : BaseManager
    {
        protected readonly AppUserRepository _appUserRepository;
        protected readonly ProviderRepository _providerRepository;
        protected readonly UserInviteRepository _userInviteRepository;

        public ProviderWorkerManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper, ILoggerFactory loggerFactory,
            AppUserRepository appUserRepository, ProviderRepository providerRepository, UserInviteRepository userInviteRepository) : base(httpContextAccessor, appUserManager, mapper, loggerFactory)
        {
            _appUserRepository = appUserRepository;
            _providerRepository = providerRepository;
            _userInviteRepository = userInviteRepository;
        }

        public async Task<ProviderWorkerDisplayModel> CreateBySlug(string slug, ProviderWorkerCreateModel model)
        {
            if (!await _providerRepository.IsWorkerInRoleBySlug(slug, _currentUserId, ProviderWorkerRole.USER_MANAGER))
                throw new AccessDeniedException(slug, typeof(ProviderWorker));

            model.Email = model.Email.ToLower();

            ProviderWorker worker = null;

            var existsUser = await _appUserRepository.FindByEmailAsync(model.Email, new System.Threading.CancellationToken());
            if (existsUser == null)
            {
                existsUser = new AppUser
                {
                    CreatedTime = DateTime.UtcNow,
                    State = true,
                    Status = UserStatus.Invited,
                    Tels = new List<MRDbIdentity.Domain.UserTel>(),
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UpdatedTime = DateTime.UtcNow,
                };

                existsUser = await _appUserRepository.Insert(existsUser);
                await _appUserManager.AddToRolesAsync(existsUser, new List<string>()
                {
                    AppUserRoleList.MANAGER,
                    AppUserRoleList.USER
                });

                var providerShort = await _providerRepository.GetShortBySlug(slug);

                var invite = await _userInviteRepository.Insert(new UserInvite
                {
                    Code = UserInviteCodeGenerator.Generate(),
                    IsByIdentity = false,
                    ProviderId = providerShort.Id,
                    ProviderName = providerShort.Name,
                    State = true,
                    UserId = existsUser.Id
                });

                _logger.LogInformation("Created new user {0} from provider {1}", existsUser.Email, providerShort.Slug);

                // TODO add invite user email
            }
            else
            {
                if (await _providerRepository.IsWorkerExistsBySlug(slug, existsUser.Id))
                    throw new EntityExistsException("Id", existsUser.Id, typeof(AppUser));

                if (!await _appUserManager.IsInRoleAsync(existsUser, AppUserRoleList.MANAGER))
                    await _appUserManager.AddToRoleAsync(existsUser, AppUserRoleList.MANAGER);


                // TODO add welcome to provider user email
            }

            worker = new ProviderWorker
            {
                Roles = model.Roles,
                UserEmail = model.Email,
                UserId = existsUser.Id
            };

            await _providerRepository.InsertWorkersBySlug(slug, worker);

            _logger.LogInformation("Added new worker {0} to provider {1} by user {2}", worker.UserEmail, slug, _currentUserEmail);

            return _mapper.Map<ProviderWorkerDisplayModel>(worker).ApplyUser(existsUser);
        }

        public async Task<ApiListResponse<ProviderWorkerDisplayModel>> GetBySlug(string slug)
        {
            if (!await _providerRepository.IsWorkerExistsBySlug(slug, _currentUserId))
                throw new AccessDeniedException(slug, typeof(ProviderWorker));

            var entities = await _providerRepository.GetWorkersBySlug(slug);
            if (entities == null || !entities.Any())
                return new ApiListResponse<ProviderWorkerDisplayModel>(0, 0);

            var models = entities.Select(x => _mapper.Map<ProviderWorkerDisplayModel>(x)).ToList();
            var users = await _appUserRepository.GetShortById(entities.Select(x => x.UserId));

            foreach (var model in models)
            {
                model.ApplyUser(users.FirstOrDefault(x => x.Id == model.UserId));
            }

            return new ApiListResponse<ProviderWorkerDisplayModel>(models, 0, 0, models.Count);
        }

        public async Task<ProviderWorkerDisplayModel> UpdateBySlug(string slug, ProviderWorkerUpdateModel model)
        {
            if (!await _providerRepository.IsWorkerInRoleBySlug(slug, _currentUserId, ProviderWorkerRole.USER_MANAGER))
                throw new AccessDeniedException("", typeof(AppUser));

            var shortUser = await _appUserRepository.GetShortById(model.UserId);

            if (shortUser == null)
                throw new EntityNotFoundException(model.UserId, typeof(AppUser));

            if (shortUser.Status == UserStatus.Blocked)
                throw new MRException(-1, "User is blocked");

            ProviderWorker entity = _mapper.Map<ProviderWorker>(model);
            entity.UserEmail = shortUser.Email;

            if (!await _providerRepository.IsWorkerExistsBySlug(slug, model.UserId))
                await _providerRepository.InsertWorkersBySlug(slug, entity);
            else
                await _providerRepository.UpdateWorkerBySlug(slug, model.UserId, model.Roles);

            if (!await _appUserManager.IsInRoleAsync(shortUser, AppUserRoleList.MANAGER))
            {
                await _appUserManager.AddToRoleAsync(shortUser, AppUserRoleList.MANAGER);
            }

            return _mapper.Map<ProviderWorkerDisplayModel>(entity).ApplyUser(shortUser);
        }

        public async Task<ApiOkResult> Delete(string slug, string userId)
        {
            if (!await _providerRepository.IsWorkerInRoleBySlug(slug, _currentUserId, ProviderWorkerRole.USER_MANAGER))
                throw new AccessDeniedException(userId, typeof(AppUser));

            if (await _providerRepository.IsWorkerInRoleBySlug(slug, userId, ProviderWorkerRole.OWNER))
                throw new MRException();

            var allWorkers = await _providerRepository.GetWorkersBySlug(slug);
            if (allWorkers.Count == 1)
                throw new MRException();

            var removeResult = await _providerRepository.RemoveWorkerBySlug(slug, userId);
            if (!removeResult)
            {
                _logger.LogError("Can not delete user {0} from provider {1}", userId, slug);
                throw new MRException();
            }

            var userWorkersCount = await _providerRepository.UserInWorkersCount(userId);
            if (userWorkersCount == 0)
            {
                var user = await _appUserRepository.GetShortById(userId);
                await _appUserManager.RemoveFromRoleAsync(user, AppUserRoleList.MANAGER);
            }

            return new ApiOkResult();
        }
    }
}
