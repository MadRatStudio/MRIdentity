using Infrastructure.Model.User;
using Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MRIdentityClient.Response;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [Route("account")]
    [Authorize]
    public class AccountController : BaseController
    {
        protected AccountManager _accountManager;

        public AccountController(ILoggerFactory loggerFactory, AccountManager accountManager) : base(loggerFactory)
        {
            _accountManager = accountManager;
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        /// <returns>User display model</returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(UserDisplayModel))]
        public async Task<IActionResult> Get()
        {
            return Ok(await _accountManager.Get());
        }

        /// <summary>
        /// Update current user
        /// </summary>
        /// <param name="model">Update user model</param>
        /// <returns>Ok</returns>
        [HttpPut]
        [ProducesResponseType(200, Type = typeof(ApiOkResult))]
        public async Task<IActionResult> Update([FromBody] UserUpdateModel model)
        {
            if (!ModelState.IsValid)
                return BadModelResponse(ModelState);

            return Ok(await _accountManager.Update(model));
        }

        /// <summary>
        /// Add tel to current user
        /// </summary>
        /// <param name="model">Tel model</param>
        /// <returns>Ok</returns>
        [HttpPut]
        [Route("tel")]
        [ProducesResponseType(200, Type = typeof(ApiOkResult))]
        public async Task<IActionResult> AddTel([FromBody] CreateUserTelModel model)
        {
            if (!ModelState.IsValid)
                return BadModelResponse(ModelState);

            return Ok(await _accountManager.AddTel(model));
        }

        /// <summary>
        /// Delete tel from current user by name
        /// </summary>
        /// <param name="name">Name of tel</param>
        /// <returns>Ok</returns>
        [HttpDelete]
        [Route("tel/{name}")]
        [ProducesResponseType(200, Type = typeof(ApiOkResult))]
        public async Task<IActionResult> DeleteTel(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest();

            return Ok(await _accountManager.DeleteTel(name));
        }

        /// <summary>
        /// Update current user email
        /// </summary>
        /// <param name="model">Email update model</param>
        /// <returns>Ok</returns>
        [HttpPut]
        [Route("email")]
        [ProducesResponseType(200, Type = typeof(ApiOkResult))]
        public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailModel model)
        {
            if (!ModelState.IsValid)
                return BadModelResponse(ModelState);

            return Ok(await _accountManager.UpdateEmail(model));
        }



        /// <summary>
        /// Close current user account
        /// </summary>
        /// <returns>Ok</returns>
        [HttpDelete]
        [ProducesResponseType(200, Type = typeof(ApiOkResult))]
        public async Task<IActionResult> CloseAccount()
        {
            return Ok(await _accountManager.CloseAccount());
        }
    }
}
