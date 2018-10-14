using System;
using System.Collections.Generic;
using System.Text;

namespace CommonApi.Response
{
    public class ApiError
    {
        public string Message { get; set; }
        public string UserMessage { get; set; }
        public long Code { get; set; }
        public object Data { get; set; }

        public ApiError() { }

        public ApiError(long code, string message) : this(code, message, null, null) { }

        public ApiError(long code, string message, object data, string userMessage = null)
        {
            Code = code;
            Message = message;
            UserMessage = userMessage;
            Data = data;
        }
    }
}
