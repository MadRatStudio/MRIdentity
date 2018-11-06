using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Infrastructure.Model.Common
{
    public class ImageModel
    {
        [Required]
        public string Key { get; set; }

        [Required]
        public string Url { get; set; }

        public string Name { get; set; }

        [Required]
        public bool IsTmp { get; set; }
    }
}
