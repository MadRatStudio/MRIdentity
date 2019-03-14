using CommonApi.Exception.Basic;
using System;

namespace CommonApi.Exception.Common
{
    public class AccessDeniedException : MRException
    {
        public AccessDeniedException(string id, Type type, string uMessage = null, global::System.Exception inner = null)
            : base((int)ExceptionCodes.NOT_FOUND, $"Access to {type.ToString()} with id {id} denied", uMessage, inner) { }
    }
}
