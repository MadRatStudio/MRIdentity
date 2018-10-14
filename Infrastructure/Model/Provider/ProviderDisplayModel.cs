using Infrastructure.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Model.Provider
{
    public class ProviderDisplayModel : ProviderShortDisplayModel
    {
        public bool DisableMessage { get; set; }
        public string Description { get; set; }
        public string BackgroundUrl { get; set; }
        public List<ProviderSocialDisplayModel> Socials { get; set; }
    }

    public class ProviderSocialDisplayModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ProviderSocialType Type { get; set; }
        public string Id { get; set; }
        public string Secret { get; set; }
    }
}
