using Manager;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [Route("{skip}/{limit}")]
        [HttpGet]
        public async Task<IActionResult> Search(int skip, int limit, [FromQuery] string q)
        {
            return Json(await _languageManager.Search(skip, limit, q));
        }
    }
}
