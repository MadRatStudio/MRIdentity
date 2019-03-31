using System;
using System.Collections.Generic;
using System.Text;

namespace CommonApi.Email.User
{
    public class UserCreateTemplateModel
    {
        public string Email { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Provider { get; set; }
        public string CallbackUrl { get; set; }
    }
}
