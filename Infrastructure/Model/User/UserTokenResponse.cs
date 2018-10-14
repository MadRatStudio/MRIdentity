using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Model.User
{
    public class UserLoginResponseModel
    {
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("image_src")]
        public string ImageSrc { get; set; }

        public UserLoginTokenResponseModel Token { get; set; }
        public List<string> Roles { get; set; }
    }

    public class UserLoginTokenResponseModel
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }

        [JsonProperty("login_provider")]
        public string LoginProvider { get; set; }
        [JsonProperty("login_provider_display")]
        public string LoginProviderDisplay { get; set; }
    }
}
