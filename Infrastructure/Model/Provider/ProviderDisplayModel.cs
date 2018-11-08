using Infrastructure.Entities;
using Infrastructure.Model.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Model.Provider
{
    public class ProviderDisplayModel : ProviderShortDisplayModel
    {
        public string DisabledMessage { get; set; }
        public string LoginRedirectUrl { get; set; }

        public ImageModel Background { get; set; }
        public List<ProviderSocialDisplayModel> Socials { get; set; }

        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string KeyWords { get; set; }
    }

    public class ProviderSocialDisplayModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ProviderSocialType Type { get; set; }
        public string Id { get; set; }
        public string Secret { get; set; }
    }
}
