using CommonApi.Exception.Basic;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonApi.Exception.Common
{
    public class ModelDamagedException : MRException
    {
        public ModelDamagedException(string property, string error) : base((int)ExceptionCodes.MODEL_DAMAGED, $"Property \"{property}\" {error}") { }
        public ModelDamagedException(string message) : base((int)ExceptionCodes.MODEL_DAMAGED, message) { }
    }
}
