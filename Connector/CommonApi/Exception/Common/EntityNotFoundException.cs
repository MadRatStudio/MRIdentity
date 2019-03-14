using CommonApi.Exception.Basic;
using System;

namespace CommonApi.Exception.Common
{
    public class EntityNotFoundException : MRException
    {
        public EntityNotFoundException(string id, Type type, string uMessage = null, global::System.Exception inner = null)
            : base((int)ExceptionCodes.NOT_FOUND, $"{type.ToString()} with id {id} not found", uMessage, inner) { }
    }
}
