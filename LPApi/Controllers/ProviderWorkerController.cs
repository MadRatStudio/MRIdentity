using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Model.Provider;
using Infrastructure.Model.User;
using Infrastructure.System.Options;
using Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MRIdentityClient.Response;

namespace IdentityApi.Controllers
{
    /// <summary>
    /// Control provider users
    /// </summary>
    [Authorize(Roles = AppUserRoleList.MANAGER + "," + AppUserRoleList.ADMIN)]
    [Route("provider_worker")]
    public class ProviderWorkerController : BaseController
    {
        protected readonly ProviderWorkerManager _providerWorkerManager;

        public ProviderWorkerController(ILoggerFactory loggerFactory, ProviderWorkerManager providerWorkerManager) : base(loggerFactory)
        {
            _providerWorkerManager = providerWorkerManager;
        }

        /// <summary>
        /// Creates new worker for provider
        /// </summary>
        /// <param name="slug">Provider slug</param>
        /// <param name="model">Worker create model</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{slug}")]
        [ProducesResponseType(200, Type = typeof(ProviderWorkerDisplayModel))]
        public async Task<IActionResult> Create(string slug, [FromBody] ProviderWorkerCreateModel model)
        {
            if (!ModelState.IsValid)
                return BadModelResponse();

            return Ok(await _providerWorkerManager.CreateBySlug(slug, model));
        }

        /// <summary>
        /// Get list of selected provider workers
        /// </summary>
        /// <param name="slug">Slug</param>
        /// <returns>List of workers</returns>
        [Route("{slug}")]
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(ApiListResponse<ProviderWorkerDisplayModel>))]
        public async Task<IActionResult> Get(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return Ok();

            return Ok(await _providerWorkerManager.GetBySlug(slug));
        }

        /// <summary>
        /// Update worker in provier
        /// </summary>
        /// <param name="slug">Provider slug</param>
        /// <param name="model">Update worker model</param>
        /// <returns></returns>
        [Route("{slug}")]
        [HttpPut]
        [ProducesResponseType(200, Type = typeof(ProviderWorkerDisplayModel))]
        public async Task<IActionResult> Update(string slug, [FromBody] ProviderWorkerUpdateModel model)
        {
            if (!ModelState.IsValid)
                return BadModelResponse();

            return Ok(await _providerWorkerManager.UpdateBySlug(slug, model));
        }

        /// <summary>
        /// Delete worker from provider
        /// </summary>
        /// <param name="slug">Provider slug</param>
        /// <param name="userId">User id</param>
        /// <returns>Ok</returns>
        [Route("{slug}/{userId}")]
        [HttpDelete]
        [ProducesResponseType(200, Type = typeof(ApiOkResult))]
        public async Task<IActionResult> Delete(string slug, string userId)
        {
            return Ok(await _providerWorkerManager.Delete(slug, userId));
        }
    }

}