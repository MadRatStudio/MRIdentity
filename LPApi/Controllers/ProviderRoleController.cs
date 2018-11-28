using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace IdentityApi.Controllers
{
    [Authorize]
    public class ProviderRoleController : BaseController
    {
        public ProviderRoleController(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }


    }
}
