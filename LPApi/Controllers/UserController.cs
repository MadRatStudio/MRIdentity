using CommonApi.Exception.Request;
using CommonApi.Exception.User;
using CommonApi.Resopnse;
using CommonApi.Response;
using Infrastructure.Model.User;
using Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [Route("admin/update/{id}")]
        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(ApiResponse<UserDataModel>))]
        public async Task<IActionResult> AdminUpdate(string id)
        {
            return Ok(await _userManager.AdminGetUserById(id));
        }

        #endregion

        [Route("login/password")]
        [AllowAnonymous]
        [ProducesResponseType(200, Type = typeof(UserLoginResponseModel))]
        [ProducesResponseType(500, Type = typeof(BadRequestException))]
        [ProducesResponseType(500, Type = typeof(LoginFailedException))]
        [HttpPost]
        public async Task<IActionResult> AuthEmail([FromBody]UserLoginModel model)
        {
            return Ok(await _userManager.TokenEmail(model));
        }

        [Route("login/facebook")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> AuthFacebook()
        {
            return Ok();
        }
    }
}
