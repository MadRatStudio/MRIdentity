using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MRIdentityClient.Exception.Basic;
using MRIdentityClient.Exception.MRSystem;
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

                if (ex is MRException)
                {
                    await HandleExceptionAsync(context, (MRException)ex);
                    return;
                }

                await HandleExceptionAsync(context, new MRSystemException());
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, MRException exception)
        {
            var result = JsonConvert.SerializeObject(new
            {
                code = exception.Code,
                message = exception.Message,
                user_message = exception.UserMessage,
            });

            var code = HttpStatusCode.InternalServerError;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
}
