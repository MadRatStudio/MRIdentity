using Infrastructure.Model.User;
using Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        /// <summary>
        /// Standart login form for selected provider
        /// </summary>
        /// <param name="model">Provider login select model</param>
        /// <returns></returns>
        [HttpPost]
        [Route("provider/email")]
        public async Task<IActionResult> LoginProvider([FromBody] UserProviderEmailLogin model)
        {
            if (!ModelState.IsValid)
                return BadModelResponse(ModelState);

            return Ok(await _loginManager.ProviderLoginEmail(HttpContext, model));
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

    }
}
