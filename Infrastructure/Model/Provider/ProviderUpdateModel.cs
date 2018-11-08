using Infrastructure.Entities;
using Infrastructure.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Infrastructure.Model.Provider
{
    public class ProviderUpdateModel
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Slug { get; set; }

        [Required]
        public bool IsVisible { get; set; }

        [Required]
        public bool IsLoginEnabled { get; set; }

        public string DisabledMessage { get; set; }

        [Required]
        public ImageModel Avatar { get; set; }

        [Required]
        public ImageModel Background { get; set; }

        // service data

        public string HomePage { get; set; }
        public string LoginRedirectUrl { get; set; }

        // end service data

        [Required]
        public string Category { get; set; }
        public List<string> Tags { get; set; }

        [Required]
        public List<ProviderTranslationUpdateModel> Translations { get; set; }

        public List<ProviderSocialUpdateModel> Socials { get; set; } = new List<ProviderSocialUpdateModel>();
    }

    public class ProviderTranslationUpdateModel
    {
        [Required]
        public string LanguageCode { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string KeyWords { get; set; }

        public bool IsDefault { get; set; } = false;
    }

    public class ProviderSocialUpdateModel
    {
        [Required]
        public string Name { get; set; }

        public string Id { get; set; }
        public string Secret { get; set; }

        [Required]
        public ProviderSocialType Type { get; set; }
    }
}
