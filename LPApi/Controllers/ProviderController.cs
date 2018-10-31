using CommonApi.Resopnse;
using CommonApi.Response;
using Infrastructure.Model.Provider;
using Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [Route("provider")]
    [Authorize(Roles = "ADMIN")]
    public class ProviderController : Controller
    {
        protected readonly ProviderManager _providerManager;

        public ProviderController(ProviderManager providerManager)
        {
            _providerManager = providerManager;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProviderUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await _providerManager.Create(model));
        }

        [HttpGet]
        [Route("{slug}")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<ProviderDisplayModel>))]
        [AllowAnonymous]
        public async Task<IActionResult> Get(string slug, [FromQuery] string languageCode = null)
        {
            return Ok(await _providerManager.GetToDisplay(slug, languageCode));
        }

        [HttpGet]
        [Route("{skip}/{limit}")]
        [AllowAnonymous]
        [ProducesResponseType(200, Type = typeof(ApiListResponse<ProviderShortDisplayModel>))]
        public async Task<IActionResult> GetList(int skip, int limit, [FromQuery] string languageCode = null, [FromQuery] string q = null)
        {
            return Ok(await _providerManager.Get(skip, limit, languageCode, q));
        }

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(200, Type = typeof(ApiResponse))]
        public async Task<IActionResult> Delete(string id)
        {
            return Ok(await _providerManager.Delete(id));
        }
        
    }
}
