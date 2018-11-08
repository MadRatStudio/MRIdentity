using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Infrastructure.Model.Provider
{
    public class ProviderRoleListUpdateModel
    {
        public List<ProviderRoleUpdateModel> Roles { get; set; }
    }

    public class ProviderRoleUpdateModel
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public bool IsDefault { get; set; }
    }
}
