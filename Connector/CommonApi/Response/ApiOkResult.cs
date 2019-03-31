using System;
using System.Collections.Generic;
using System.Text;

namespace CommonApi.Response
{
    public class ApiOkResult
    {
        public bool Success { get; set; }

        public ApiOkResult() : this(false) { }
        public ApiOkResult(bool result)
        {
            Success = result;
        }
    }
}
