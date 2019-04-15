using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Model.User
{
    public class UserCreateModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public List<UserCreateTelModel> Tels { get; set; }
    }

    public class UserCreateTelModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Number { get; set; }
    }
}
