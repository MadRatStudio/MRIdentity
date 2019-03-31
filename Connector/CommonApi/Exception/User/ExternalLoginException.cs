using CommonApi.Exception.Basic;

namespace CommonApi.Exception.User
{
    public class ExternalLoginException : MRException
    {
        public ExternalLoginException() : base((int)ExceptionCodes.EXTERNAL_LOGIN_FAILED, "External login failed") { }
    }
}
