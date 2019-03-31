using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Model.User
{
    public class UserResetPasswordModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
