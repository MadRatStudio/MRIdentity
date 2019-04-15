using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Model.Provider;
using Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MRIdentityClient.Response;

namespace IdentityApi.Controllers
{
    [Authorize]
    [Route("provider_role")]
    public class ProviderRoleController : BaseController
    {
        protected readonly ProviderRoleManager _providerRoleManager;

        public ProviderRoleController(ILoggerFactory loggerFactory, ProviderRoleManager providerRoleManager) : base(loggerFactory)
        {
            _providerRoleManager = providerRoleManager;
        }

        /// <summary>
        /// List of roles for target provider
        /// </summary>
        /// <param name="slug">Slug of provider</param>
        /// <returns>List provider role display model</returns>
        [HttpGet]
        [Route("list/{slug}")]
        [ProducesResponseType(200, Type = typeof(ApiListResponse<ProviderRoleDisplayModel>))]
        public async Task<IActionResult> Get(string slug)
        {
            return Ok(await _providerRoleManager.GetProviderRoles(slug));
        }

        /// <summary>
        /// Add new role to provider
        /// </summary>
        /// <param name="slug">Slug of provider</param>
        /// <param name="model">Create role model</param>
        /// <returns>Role display model</returns>
        [HttpPut]
        [Route("{slug}")]
        [ProducesResponseType(200, Type = typeof(ProviderRoleDisplayModel))]
        public async Task<IActionResult> Create(string slug, [FromBody] ProviderRoleCreateModel model)
        {
            if (!ModelState.IsValid)
                return BadModelResponse(ModelState);

            return Ok(await _providerRoleManager.CreateProviderRole(slug, model));
        }

        /// <summary>
        /// Delete role from provider
        /// </summary>
        /// <param name="slug">Slug of provider</param>
        /// <param name="name">Name of deleting role</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{slug}/{name}")]
        [ProducesResponseType(200, Type = typeof(ApiOkResult))]
        public async Task<IActionResult> Delete(string slug, string name)
        {
            return Ok(await _providerRoleManager.RemoveProviderRole(slug, name));
        }
    }
}
