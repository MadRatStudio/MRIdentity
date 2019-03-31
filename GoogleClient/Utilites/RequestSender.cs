using GoogleClient.Infrastructure.Common;
using GoogleClient.Infrastructure.Request;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GoogleClient.Utilites
{
    public class UserRequestSender
    {
        protected JsonSerializerSettings _jsonSettings => new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            Formatting = Formatting.Indented
        };

        public async Task<ResponseModel<T>> Get<T>(RequestModel request)
            where T : class, new()
        {
            var client = new HttpClient();
            var message = new HttpRequestMessage(HttpMethod.Get, request.Url);
            message.Headers.Add(HttpRequestHeader.Accept.ToString(), "application/json");

            if (!string.IsNullOrWhiteSpace(request.Token))
            {
                message.Headers.Add(HttpRequestHeader.Authorization.ToString(), $"Bearer {request.Token}");
            }

            try
            {
                var response = await client.SendAsync(message);
                if (!response.IsSuccessStatusCode)
                {
                    throw new SystemException("Unsuccess");
                }

                var data = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync(), _jsonSettings);
                return new ResponseModel<T>(data);

            }
            catch (Exception ex)
            {
                return new ResponseModel<T>(ex);
            }
        }

    }
}
