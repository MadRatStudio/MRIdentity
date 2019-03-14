using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

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

        public ApiListResponse() { }

        public ApiListResponse(int skip, int limit)
        {
            Skip = skip;
            Limit = limit;
        }

        public ApiListResponse(IEnumerable<T> data, int take, int limit, long total)
        {
            Data = data?.ToList();
            Limit = take;
            Skip = limit;
            Total = total;
        }
    }
}
