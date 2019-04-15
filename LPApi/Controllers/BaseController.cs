using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using MRIdentityClient.Exception.Common;

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
