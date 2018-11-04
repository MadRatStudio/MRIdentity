using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Infrastructure.Model.Provider
{
    public class ProviderFingerprintCreateModel
    {
        [Required]
        public string Name { get; set; }
        public string Domain { get; set; }
    }
}
