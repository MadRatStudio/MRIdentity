using Newtonsoft.Json;

namespace CommonApi.Models
{
    public class IdNameModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public IdNameModel()
        {

        }

        public IdNameModel(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
