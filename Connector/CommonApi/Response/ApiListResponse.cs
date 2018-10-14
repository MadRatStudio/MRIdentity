using CommonApi.Resopnse;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonApi.Response
{
    public class ApiListResponse<T> 
    {
        [JsonProperty("data")]
        public List<T> Data { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("skip")]
        public int Skip { get; set; }

        [JsonProperty("error")]
        public ApiError Error { get; set; }

        public ApiListResponse() { }

        public ApiListResponse(int skip, int limit)
        {
            Skip = skip;
            Limit = limit;
        }

        public ApiListResponse(IEnumerable<T> data, int take, int limit, long total)
        {
            Error = null;
            Data = data?.ToList();
            Limit = take;
            Skip = limit;
            Total = total;
        }

        public ApiListResponse(ApiError error)
        {
            Error = error;
        }
    }
}
