using CommonApi.Exception.Basic;

namespace CommonApi.Exception.User
{
    public class LoginFailedException : MRException
    {
        public LoginFailedException(string email) : base((int)ExceptionCodes.LOGIN_FAILED, $"Login for user {email} failed.") { }
    }
}
