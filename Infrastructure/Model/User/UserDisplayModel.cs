using Infrastructure.Model.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Model.User
{
    public class UserDisplayModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public ImageModel Avatar { get; set; }

        public List<UserTelDisplayModel> Tels { get; set; }
    }

    public class UserTelDisplayModel
    {
        public string Name { get; set; }
        public string Number { get; set; }
    }

    public class UserConnectedProviderDisplayModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public DateTime LastLoginTime { get; set; }
    }
}
