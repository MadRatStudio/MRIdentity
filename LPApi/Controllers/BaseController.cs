using CommonApi.Errors;
using CommonApi.Resopnse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    public class BaseController : Controller
    {
        protected IActionResult BadModelResponse(ModelStateDictionary pairs)
        {
            ApiResponse response = new ApiResponse
            {
                Response = null,
                Error = ECollection.MODEL_DAMAGED
            };

            if(pairs.ErrorCount > 0)
            {
                var error = pairs.First();
                response.Error.Data = new CommonApi.Errors.ModelError
                {
                    Property = error.Key,
                    Error = error.Value.Errors.First().ErrorMessage
                };
            }

            return Ok(response);
        }
    }
}
