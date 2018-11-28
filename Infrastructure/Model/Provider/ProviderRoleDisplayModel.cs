using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Model.Provider
{
    public class ProviderRoleDisplayModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }

    public class ProviderRoleCreateModel
    {
        [Required]
        public string Name { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}
