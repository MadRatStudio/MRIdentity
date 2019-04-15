using Infrastructure.Entities.Enum;
using Infrastructure.Model.User;
using Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MRIdentityClient.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [Route("user")]
    [Authorize(Roles = UserRoles.ADMIN)]
    public class UserController : BaseController
    {
        protected UserManager _userManager;

        public UserController(ILoggerFactory loggerFactory, UserManager userManager) : base(loggerFactory)
        {
            _userManager = userManager;
        }

        #region admin

        [HttpPost]
        [Route("admin")]
        [ProducesResponseType(200, Type = typeof(UserShortDataModel))]
        public async Task<IActionResult> Create([FromBody] UserCreateModel model)
        {
            return Ok(await _userManager.AdminCreate(model));
        }

        [Route("admin/list/{skip}/{limit}")]
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(ApiListResponse<UserShortDataModel>))]
        public async Task<IActionResult> AdminList(int skip, int limit, [FromQuery] string q = null)
        {
            return Ok(await _userManager.AdminGetCollection(skip, limit, q));
        }

        [Route("admin/roles/{id}")]
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<string>))]
        public async Task<IActionResult> GetRoles(string id)
        {
            return Ok(await _userManager.GetRoles(id));
        }

        [Route("admin/update/{id}")]
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(UserDataModel))]
        public async Task<IActionResult> AdminUpdate(string id)
        {
            return Ok(await _userManager.AdminGetUserById(id));
        }

        [HttpPut]
        [Route("admin/update/{id}")]
        [ProducesResponseType(200, Type = typeof(UserCreateModel))]
        public async Task<IActionResult> AdminUpdate(string id, [FromBody] UserCreateModel model)
        {
            return Ok();
        }

        [HttpPut]
        [Route("admin/roles/{id}")]
        [ProducesResponseType(200, Type = typeof(OkObjectResult))]
        public async Task<IActionResult> UserUpdateRoles(string id, [FromBody] UserRoleUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadModelResponse();
            }

            return Ok(await _userManager.UpdateRoles(id, model));
        }

        [HttpDelete]
        [Route("admin/{id}")]
        [ProducesResponseType(200, Type = typeof(OkResult))]
        public async Task<IActionResult> AdminDelete(string id)
        {
            return Ok(await _userManager.AdminDelete(id));
        }
        #endregion
    }
}
