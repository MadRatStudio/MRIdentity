using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Template.User
{
    public class ResetPasswordModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string Url { get; set; }
    }
}
