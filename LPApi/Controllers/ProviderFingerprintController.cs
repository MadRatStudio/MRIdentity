using Infrastructure.Model.Provider;
using Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MRIdentityClient.Response;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [Route("provider_fingerprint")]
    [Authorize(Roles = "ADMIN, MANAGER")]
    public class ProviderFingerprintController : BaseController
    {
        protected ProviderManager _providerManager;

        public ProviderFingerprintController(ILoggerFactory loggerFactory, ProviderManager providerManager) : base(loggerFactory)
        {
            _providerManager = providerManager;
        }

        /// <summary>
        /// Get fingerprints
        /// </summary>
        /// <param name="id">Provider id</param>
        /// <returns>Api list response of Fingerprints display models</returns>
        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(ApiListResponse<ProviderFingerprintDisplayModel>))]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            return Ok(await _providerManager.GetProviderFingerprints(id));
        }

        /// <summary>
        /// Create fingerprint
        /// </summary>
        /// <param name="id">Provider id</param>
        /// <param name="model"></param>
        /// <returns>Fingerprint display model</returns>
        [HttpPost]
        [Route("{id}")]
        [ProducesResponseType(200, Type = typeof(ProviderFingerprintDisplayModel))]
        public async Task<IActionResult> Create(string id, [FromBody] ProviderFingerprintCreateModel model)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            if (!ModelState.IsValid)
                return BadModelResponse(ModelState);

            return Ok(await _providerManager.CreateFingerprint(id, model));
        }

        /// <summary>
        /// Delete fingerprint
        /// </summary>
        /// <param name="id">provider id</param>
        /// <param name="name">fingerprint name</param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(200, Type = typeof(OkResult))]
        [Route("{id}/{name}")]
        public async Task<IActionResult> Delete(string id, string name)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                return BadRequest();

            await _providerManager.DeleteFingerprint(id, name);

            return Ok();
        }
    }
}
