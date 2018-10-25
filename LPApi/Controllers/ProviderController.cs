using CommonApi.Resopnse;
using CommonApi.Response;
using Infrastructure.Model.Provider;
using Manager;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [Route("provider")]
    public class ProviderController : Controller
    {
        protected readonly ProviderManager _providerManager;

        public ProviderController(ProviderManager providerManager)
        {
            _providerManager = providerManager;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Provider model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await _providerManager.Create(model));
        }

        [HttpGet]
        [Route("{skip}/{limit}")]
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
