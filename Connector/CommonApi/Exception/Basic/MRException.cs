using Newtonsoft.Json;
using System;

namespace CommonApi.Exception.Basic
{
    [Serializable]
    public class MRException : System.Exception
    {
        public int Code { get; set; }
        public string UserMessage { get; set; }
        
        [JsonIgnore]
        new public System.Exception InnerException { get; set; }

        public MRException() : this(-1, "System error", "System error", null) { }
        public MRException(int code, string message) : this(code, message, message, null) { }
        public MRException(int code, string message, string userMessage, System.Exception innerException) : base(message)
        {
            Code = code;
            UserMessage = userMessage;
            InnerException = innerException;
        }
    }
}
