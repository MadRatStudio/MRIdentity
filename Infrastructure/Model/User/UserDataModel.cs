using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Model.User
{
    public class UserDataModel : UserShortDataModel
    {
        public List<UserDataSocialModel> Socials { get; set; }
        public List<UserDataProviderModel> ConnectedProviders { get; set; }
        public List<UserDataTel> Tels { get; set; }
        public List<string> Roles { get; set; }
    }

    public class UserShortDataModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsBlocked { get; set; }
        public string Email { get; set; }
        public string AvatarSrc { get; set; }

        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
    }

    public class UserDataSocialModel
    {
        public string Name { get; set; }
        public string Token { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class UserDataProviderModel
    {
        public string ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string UserIp { get; set; }
        public string BrowserMeta { get; set; }
    }

    public class UserDataTel
    {
        public string Name { get; set; }
        public string Number { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
