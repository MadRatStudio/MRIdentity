using Infrastructure.Model.Provider;
using Infrastructure.Model.User;
using Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MRIdentityClient.Exception.Request;
using MRIdentityClient.Exception.User;
using MRIdentityClient.Models.User;
using MRIdentityClient.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [Route("login")]
    public class LoginController : BaseController
    {
        protected readonly LoginManager _loginManager;

        public LoginController(ILoggerFactory loggerFactory, LoginManager loginManager) : base(loggerFactory)
        {
            _loginManager = loginManager;
        }

        #region login


        [HttpPost]
        [Route("email")]
        [AllowAnonymous]
        [ProducesResponseType(200, Type = typeof(UserLoginResponseModel))]
        [ProducesResponseType(500, Type = typeof(BadRequestException))]
        [ProducesResponseType(500, Type = typeof(LoginFailedException))]
        public async Task<IActionResult> AuthEmail([FromBody]UserLoginModel model)
        {
            return Ok(await _loginManager.LoginEmail(model));
        }

        [HttpPost]
        [Route("external/facebook")]
        [AllowAnonymous]
        public async Task<IActionResult> AuthFacebook()
        {
            return Ok();
        }

        [HttpPost]
        [Route("external/google")]
        [AllowAnonymous]
        [ProducesResponseType(200, Type = typeof(UserLoginResponseModel))]
        public async Task<IActionResult> AuthGoogle([FromBody] UserGoogleAuthModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadModelResponse();
            }

            return Ok(await _loginManager.LoginGoogle(model));
        }
        #endregion

        #region Signup

        [HttpPost]
        [Route("signup/email")]
        [AllowAnonymous]
        public async Task<IActionResult> SignupEmail([FromBody] UserSignupModel model)
        {
            if (!ModelState.IsValid)
                return BadModelResponse();

            return Ok(await _loginManager.SignupEmail(model));
        }

        #endregion

        /// <summary>
        /// Standart login form for selected provider
        /// </summary>
        /// <param name="model">Provider login select model</param>
        /// <returns></returns>
        [HttpPost]
        [Route("provider/email")]
        [ProducesResponseType(200, Type = typeof(ProviderTokenResponse))]
        public async Task<IActionResult> LoginProvider([FromBody] UserProviderEmailLogin model)
        {
            if (!ModelState.IsValid)
                return BadModelResponse(ModelState);

            return Ok(await _loginManager.ProviderLoginEmail(HttpContext, model));
        }

        [HttpPut]
        [Route("provider/approve")]
        [AllowAnonymous]
        [ProducesResponseType(200, Type = typeof(ApproveLogin))]
        public async Task<IActionResult> LoginApprove([FromQuery] string token, [FromQuery] string fingerprint)
        {
            return Ok(await _loginManager.ProviderApproveLogin(HttpContext, token, fingerprint));
        }

        /// <summary>
        /// Instant login from your account to target provider
        /// </summary>
        /// <param name="id">Target provider id</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("provider/instant/{id}")]
        public async Task<IActionResult> LoginProviderInstant(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            return Ok(await _loginManager.ProviderLoginInstant(HttpContext, id));
        }

        [HttpGet]
        [Route("reset_password/{email}")]
        [ProducesResponseType(200, Type = typeof(OkResult))]
        public async Task<IActionResult> ResetPassword(string email)
        {
            return Ok(await _loginManager.ResetPasswordRequest(email));
        }

        [HttpPut]
        [Route("reset_password")]
        [ProducesResponseType(200, Type = typeof(ApiOkResult))]
        public async Task<IActionResult> ApproveResetPassword([FromBody] UserResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadModelResponse();

            return Ok(await _loginManager.ResetPasswordApprove(model));
        }

    }
}
