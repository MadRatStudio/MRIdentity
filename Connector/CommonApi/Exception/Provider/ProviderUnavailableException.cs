using CommonApi.Exception.Basic;

namespace CommonApi.Exception.Provider
{
    public class ProviderUnavaliableException : MRException
    {
        public ProviderUnavaliableException(string name) : base((int)ExceptionCodes.PROVIDER_UNAVAILABLE, $"Provider {name} is unavailable.") { }
    }
}
