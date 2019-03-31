using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Model.User
{
    public class UserCreateModel
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public List<UserCreateTelModel> Tels { get; set; }
    }

    public class UserCreateTelModel
    {
        public string Name { get; set; }
        public string Number { get; set; }
    }
}
