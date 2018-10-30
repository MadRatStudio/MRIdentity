using CommonApi.Response;

namespace CommonApi.Errors
{
    public static class ECollection
    {
        // basic errors with start 0
        public static ApiError UNSUPPORTED_REQUEST = new ApiError
        {
            Code = 0,
            Message = "Unsupported request",
            UserMessage = "Unsupported request"
        };


        // auth errors
        public static ApiError NOT_AUTHORIZED = new ApiError
        {
            Code = 100,
            Message = "Not authorized",
            UserMessage = "You need to be authorized for this action"
        };

        public static ApiError ACCESS_DENIED = new ApiError
        {
            Code = 101,
            Message = "Access denied",
            UserMessage = "Access denied for this action"
        };

        public static ApiError USER_NOT_FOUND = new ApiError
        {
            Code = 102,
            Message = "User not found",
            UserMessage = "User not found"
        };


        // model damaged

        public static ApiError MODEL_DAMAGED = new ApiError
        {
            Code = 200,
            Message = "Model damaged",
            UserMessage = "Bad data accept"
        };

        public static ApiError ENTITY_EXISTS = new ApiError
        {
            Code = 201,
            Message = "Entity already exists",
            UserMessage = "Entity already exists"
        };

        public static ApiError ENTITY_NOT_FOUND = new ApiError
        {
            Code = 202,
            Message = "Entity not found",
            UserMessage = "Requested entity not found"
        };

        public static ApiError BAD_DATA_FORMAT = new ApiError
        {
            Code = 203,
            Message = "Bad data format",
            UserMessage = "Requested data format in not valid"
        };

        // undefined error

        public static ApiError UNDEFINED_ERROR = new ApiError
        {
            Code = 900,
            Message = "Undefined server error",
            UserMessage = "Undefined server error. Connect to administrator"
        };


        public static ApiError Select(ApiError error, object model = null, string localization = null)
        {
            if (error == null) return null;

            error.Data = model;
            if (!string.IsNullOrWhiteSpace(localization))
            {
                error.UserMessage = localization;
            }

            return error;
        }

    }

}
