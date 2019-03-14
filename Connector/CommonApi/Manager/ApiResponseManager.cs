using CommonApi.Errors;
using CommonApi.Resopnse;
using CommonApi.Response;

namespace CommonApi.Manager
{
    public class ApiResponseManager
    {
        public ApiResponseManager()
        {

        }

        public ApiResponse Ok(object response = null)
        {
            return new ApiResponse
            {
                Response = response,
                Error = null
            };
        }
        public ApiResponse<T> Ok<T>(T obj)
        {
            return new ApiResponse<T>
            {
                Response = obj
            };
        }

        public ApiResponse Fail(long code, string message, object errorData = null, string userMessage = null)
        {
            return new ApiResponse
            {
                Response = null,
                Error = new Response.ApiError(code, message, errorData, userMessage)
            };
        }

        public ApiResponse Fail(ApiError error)
        {
            return new ApiResponse
            {
                Response = null,
                Error = error
            };
        }

        public ApiResponse<T> Fail<T>(long code, string message, object errorData = null, string userMessage = null)
        {
            return new ApiResponse<T>
            {
                Response = default(T),
                Error = new ApiError(code, message, errorData, userMessage)
            };
        }

        public ApiResponse<T> Fail<T>(ApiError error)
        {
            return new ApiResponse<T>
            {
                Response = default(T),
                Error = error
            };
        }

        public ApiListResponse<T> FailList<T>(ApiError error)
        {
            return new ApiListResponse<T>
            {
                Data = new System.Collections.Generic.List<T>(),
                Total = 0,
                Skip = 0,
                Limit = 0
            };
        }
    }
}
