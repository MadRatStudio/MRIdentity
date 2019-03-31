namespace CommonApi.Exception
{
    public enum ExceptionCodes
    {
        SYSTEM_EXCEPTION = -1,

        BAD_REQUEST = 1,
        MODEL_DAMAGED = 2,
        BAD_MODEL = 3,

        NOT_FOUND = 100,
        ACCESS_DENIED = 101,
        NOT_ENOUGH_RIGHTS = 102,

        USER_NOT_FOUND = 200,
        USER_DAMAGED = 201,
        LOGIN_FAILED = 202,
        EXTERNAL_LOGIN_FAILED = 203,

        PROVIDER_ERROR = 300,
        PROVIDER_UNAVAILABLE = 301,
        PROVIDER_CLOSED = 302,
    }
}
