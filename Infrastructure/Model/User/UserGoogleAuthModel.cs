using System;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Model.User
{
    public class UserGoogleAuthModel
    {
        [Required]
        public string Data { get; set; }

        [Required]
        public DateTime Expiry { get; set; }

        [Required]
        public string Type { get; set; }
    }
}
