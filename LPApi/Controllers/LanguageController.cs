using Infrastructure.Model.Common;
using Manager;
using Microsoft.AspNetCore.Mvc;
using MRIdentityClient.Response;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [Route("language")]
    public class LanguageController : Controller
    {
        protected readonly LanguageManager _languageManager;

        public LanguageController(LanguageManager languageManager)
        {
            _languageManager = languageManager;
        }

        [HttpGet]
        [Route("{skip}/{limit}")]
        [ProducesResponseType(200, Type = typeof(ApiListResponse<LanguageDisplayModel>))]
        public async Task<IActionResult> Search(int skip, int limit, [FromQuery] string q)
        {
            return Ok(await _languageManager.Search(skip, limit, q));
        }

        [HttpGet]
        [Route("all")]
        [ProducesResponseType(200, Type = typeof(ApiListResponse<LanguageDisplayModel>))]
        public async Task<IActionResult> All()
        {
            return Ok(await _languageManager.All());
        }
    }
}
