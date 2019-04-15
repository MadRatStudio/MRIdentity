using Infrastructure.Model.Provider;
using Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MRIdentityClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [Route("tag")]
    public class TagController : Controller
    {
        protected readonly TagManager _tagManager;

        public TagController(TagManager tagManager)
        {
            _tagManager = tagManager;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN, MANAGER")]
        [ProducesResponseType(200, Type = typeof(IdNameModel))]
        public async Task<IActionResult> Create([FromBody] ProviderTagCreateModel model)
        {
            return Json(await _tagManager.Create(model));
        }

        [HttpGet]
        [Route("{skip}/{take}")]
        public async Task<IActionResult> Get(int skip, int limit, [FromQuery] string q = null, [FromQuery] string languageCode = null)
        {
            return Json(await _tagManager.Get(skip, limit, languageCode, q));
        }
    }
}
