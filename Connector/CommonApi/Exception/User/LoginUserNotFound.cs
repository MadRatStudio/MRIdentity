using CommonApi.Exception.Basic;

namespace CommonApi.Exception.User
{
    public class LoginUserNotFound : MRException
    {
        public LoginUserNotFound(string email) : base((int)ExceptionCodes.LOGIN_FAILED, $"User with email {email} not found.") { }
    }
}
