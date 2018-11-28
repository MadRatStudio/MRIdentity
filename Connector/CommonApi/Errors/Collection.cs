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

        public static ApiError PROVIDER_NOT_FOUND = new ApiError
        {
            Code = 103,
            Message = "Provider not found",
            UserMessage = "Requested provider not found"
        };

        public static ApiError PROVIDER_UNAVALIABLE = new ApiError
        {
            Code = 104,
            Message = "Provider unavaliable",
            UserMessage = "Requested provider is unavaliable now"
        };

        public static ApiError USER_BLOCKED = new ApiError
        {
            Code = 105,
            Message = "User is blocked",
            UserMessage = "Current user is blocked"
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

        public static ApiError TRANSFER_IMAGE_ERROR = new ApiError
        {
            Code = 204,
            Message = "Image upload erro",
            UserMessage = "Can not upload image"
        };

        public static ApiError PROPERTY_REQUIRED = new ApiError
        {
            Code = 205,
            Message = "Propery required",
            UserMessage = "Property required"
        };


        // token validation failed

        public static ApiError TOKEN_VALIDATION_FAILED = new ApiError
        {
            Code = 300,
            Message = "Token validation failed",
            UserMessage = "Token validation failed"
        };

        public static ApiError TOKEN_CHALLENGE_FAILED = new ApiError
        {
            Code = 301,
            Message = "Token challenge failed",
            UserMessage = "Token challenge failed"
        };

        public static ApiError TOKEN_PROVIDER_NOT_ALLOWED = new ApiError
        {
            Code = 302,
            Message = "Provider not allowed",
            UserMessage = "Provider do not allow login"
        };

        public static ApiError TOKEN_PROVIDER_NOT_FOUND = new ApiError
        {
            Code = 303,
            Message = "Provider not found",
            UserMessage = "Provider do not found"
        };

        // provider action errors

        public static ApiError USER_ALREADY_CONNECTED = new ApiError
        {
            Code = 400,
            Message = "User already connected",
            UserMessage = "User already connected to target provider"
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
