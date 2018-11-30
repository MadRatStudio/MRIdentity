using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Model.Template.User
{
    public class ProviderInviteTemplate
    {
        public string ProviderName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LoginLink { get; set; }
    }
}
