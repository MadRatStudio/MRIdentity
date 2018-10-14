using System;
using System.Collections.Generic;
using System.Text;

namespace CommonApi.Errors
{
    public class ModelError
    {
        public string Property { get; set; }
        public string Error { get; set; }

        public ModelError()
        {

        }

        public ModelError(string property, string error)
        {
            Property = property;
            Error = error;
        }
    }
}
