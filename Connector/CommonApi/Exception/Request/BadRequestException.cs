using CommonApi.Exception.Basic;

namespace CommonApi.Exception.Request
{
    public class BadRequestException : MRException
    {
        public BadRequestException() : base((int)ExceptionCodes.BAD_REQUEST, "Bad request") { }
    }
}
