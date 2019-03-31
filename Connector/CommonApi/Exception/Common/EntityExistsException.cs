using CommonApi.Exception.Basic;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonApi.Exception.Common
{
    public class EntityExistsException : MRException
    {
        public EntityExistsException(string propertyName, string propertyValue, Type type, string uMessage = null, global::System.Exception inner = null)
            : base((int)ExceptionCodes.NOT_FOUND, $"Entity of type {type.ToString()} with {propertyName} {propertyValue} already exists.", uMessage, inner) { }
    }
}
