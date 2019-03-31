using CommonApi.Errors;
using CommonApi.Exception.Common;
using CommonApi.Resopnse;
using CommonApi.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    public class BaseController : Controller
    {
        protected ILogger _logger;

        public BaseController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        protected IActionResult BadModelResponse() => BadModelResponse(ModelState);
        protected IActionResult BadModelResponse(ModelStateDictionary state)
        {
            throw new BadModelException(state);
        }
    }
}
