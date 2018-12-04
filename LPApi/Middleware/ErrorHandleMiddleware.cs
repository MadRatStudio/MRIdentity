using CommonApi.Errors;
using CommonApi.Resopnse;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace IdentityApi.Middleware
{
    public class ErrorHandlerMiddleware
    {
        protected readonly RequestDelegate _next;
        protected readonly Logger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = new Logger<ErrorHandlerMiddleware>(loggerFactory);
        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server error");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.OK; // 500 if unexpected

            var result = JsonConvert.SerializeObject(new ApiResponse
            {
                Error = ECollection.UNDEFINED_ERROR
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
}
