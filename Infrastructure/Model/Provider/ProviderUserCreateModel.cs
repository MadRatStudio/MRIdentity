using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Infrastructure.Model.Provider
{
    public class ProviderUserCreateModel
    {
        [Required]
        public string ProviderId { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        /// <summary>
        /// User role names
        /// </summary>
        [Required]
        public List<string> Roles { get; set; }
    }

    public class ProviderUserUpdateModel
    {
        [Required]
        public string ProviderId { get; set; }

        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// User role names
        /// </summary>
        [Required]
        public List<string> Roles { get; set; }
    }
}
