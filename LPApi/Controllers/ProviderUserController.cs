using System.Threading.Tasks;
using Infrastructure.Model.Provider;
using Infrastructure.Model.User;
using Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MRIdentityClient.Response;

namespace IdentityApi.Controllers
{
    /// <summary>
    /// Control provider users
    /// </summary>
    [Authorize]
    [Route("provider_user")]
    public class ProviderUserController : BaseController
    {
        protected readonly ProviderUserManager _providerUserManager;

        public ProviderUserController(ILoggerFactory loggerFactory, ProviderUserManager providerUserManager) : base(loggerFactory)
        {
            _providerUserManager = providerUserManager;
        }

        /// <summary>
        /// Invite new user to service and provider
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(200, Type = typeof(UserDisplayModel))]
        public async Task<IActionResult> Create([FromBody]ProviderUserCreateModel model)
        {
            if (!ModelState.IsValid)
                return BadModelResponse();

            return Ok(await _providerUserManager.InviteUser(model));
        }

        /// <summary>
        /// Update user roles
        /// </summary>
        /// <param name="model">User update model</param>
        /// <returns>Ok result</returns>
        [HttpPut]
        [ProducesResponseType(200, Type = typeof(ApiOkResult))]
        public async Task<IActionResult> Update([FromBody] ProviderUserUpdateModel model)
        {
            if (!ModelState.IsValid)
                return BadModelResponse();

            return Ok(await _providerUserManager.UpdateUserRoles(model));
        }

        
    }

}