using CommonApi.Exception.Request;
using CommonApi.Exception.User;
using CommonApi.Resopnse;
using CommonApi.Response;
using Infrastructure.Model.User;
using Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [Route("user")]
    [Authorize]
    public class UserController : Controller
    {
        protected UserManager _userManager;

        public UserController(UserManager userManager)
        {
            _userManager = userManager;
        }

        #region admin

        [Route("admin/list/{skip}/{limit}")]
        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(ApiListResponse<UserShortDataModel>))]
        public async Task<IActionResult> AdminList(int skip, int limit, [FromQuery] string q = null)
        {
            return Ok(await _userManager.AdminGetCollection(skip, limit, q));
        }

        [Route("admin/roles/{id}")]
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(200, Type = typeof(List<string>))]
        public async Task<IActionResult> GetRoles(string id)
        {
            return Ok(await _userManager.GetRoles(id));
        }

        [Route("admin/update/{id}")]
        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(UserDataModel))]
        public async Task<IActionResult> AdminUpdate(string id)
        {
            return Ok(await _userManager.AdminGetUserById(id));
        }

        [HttpPut]
        [Route("admin/update/{id}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(200, Type = typeof(UserCreateModel))]
        public async Task<IActionResult> AdminUpdate(string id, [FromBody] UserCreateModel model)
        {
            return Ok();
        }

        [HttpPost]
        [Route("admin")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(200, Type = typeof(UserShortDataModel))]
        public async Task<IActionResult> Create([FromBody] UserCreateModel model)
        {
            return Ok(await _userManager.AdminCreate(model));
        }

        [HttpDelete]
        [Route("admin/{id}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(200, Type = typeof(OkResult))]
        public async Task<IActionResult> AdminDelete(string id)
        {
            return Ok(await _userManager.AdminDelete(id));
        }
        #endregion
    }
}
