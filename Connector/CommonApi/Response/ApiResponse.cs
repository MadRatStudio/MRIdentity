using CommonApi.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonApi.Resopnse
{
    public class ApiResponse
    {
        [JsonProperty("response")]
        public object Response { get; set; }

        [JsonProperty("error")]
        public ApiError Error { get; set; }

    }

    public class ApiResponse<T>
    {
        [JsonProperty("response")]
        public T Response { get; set; }

        [JsonProperty("error")]
        public ApiError Error { get; set; }

        public static implicit operator ApiResponse<T>(ApiResponse a)
        {
            return new ApiResponse<T>
            {
                Response = (T)a.Response,
                Error = a.Error
            };
        }

    }
}
