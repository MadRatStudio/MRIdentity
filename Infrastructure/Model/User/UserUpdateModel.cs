using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Model.User
{
    public class UserUpdateModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public List<UserUpdateTelModel> Tels { get; set; }
    }

    public class UserUpdateTelModel
    {
        public string Name { get; set; }
        public string Number { get; set; }
    }
}
