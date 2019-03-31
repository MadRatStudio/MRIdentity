using CommonApi.Exception.Basic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace CommonApi.Exception.Common
{
    public class BadModelException : MRException
    {
        public BadModelException(ModelStateDictionary state) : base((int)ExceptionCodes.BAD_MODEL, $"{state.First().Key} {state.First().Value.Errors.First().ErrorMessage}") { }
        public BadModelException(string message) : base((int)ExceptionCodes.BAD_MODEL, message) { }
    }
}
