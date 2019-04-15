using Infrastructure.Model.Provider;
using Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MRIdentityClient.Models;
using MRIdentityClient.Response;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [Route("provider")]
    [Authorize(Roles = "ADMIN")]
    public class ProviderController : BaseController
    {
        protected readonly ProviderManager _providerManager;

        public ProviderController(ILoggerFactory loggerFactory, ProviderManager providerManager) : base(loggerFactory)
        {
            _providerManager = providerManager;
        }

        [HttpGet]
        [Route("{skip}/{limit}")]
        [AllowAnonymous]
        [ProducesResponseType(200, Type = typeof(ApiListResponse<ProviderShortDisplayModel>))]
        public async Task<IActionResult> GetList(int skip, int limit, [FromQuery] string languageCode = null, [FromQuery] string q = null)
        {
            return Ok(await _providerManager.Get(skip, limit, languageCode, q));
        }

        [HttpPost]
        [ProducesResponseType(200, Type = typeof(IdNameModel))]
        public async Task<IActionResult> Create([FromBody] ProviderUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadModelResponse(ModelState);
            }

            return Ok(await _providerManager.Create(model));
        }

        [HttpGet]
        [Route("{slug}")]
        [ProducesResponseType(200, Type = typeof(ProviderDisplayModel))]
        [AllowAnonymous]
        public async Task<IActionResult> Get(string slug, [FromQuery] string languageCode = null)
        {
            _logger.LogError("Log Error");
            _logger.LogInformation("Some info");
            return Ok(await _providerManager.GetToDisplay(slug, languageCode));
        }

        [HttpGet]
        [Route("update/{slug}")]
        [ProducesResponseType(200, Type = typeof(ProviderUpdateModel))]
        public async Task<IActionResult> GetUpdateModel(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return BadRequest();

            return Ok(await _providerManager.GetUpdateModel(slug));
        }

        [HttpPut]
        [ProducesResponseType(200, Type = typeof(OkResult))]
        public async Task<IActionResult> Update([FromBody] ProviderUpdateModel model)
        {
            if (!ModelState.IsValid)
                return BadModelResponse(ModelState);

            return Ok(await _providerManager.Update(model));
        }

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(200, Type = typeof(OkResult))]
        public async Task<IActionResult> Delete(string id)
        {
            await _providerManager.Delete(id);
            return Ok();
        }
        
    }
}
