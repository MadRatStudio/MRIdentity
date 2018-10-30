using Infrastructure.Entities;
using Infrastructure.Model.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Model.Provider
{
    public class ProviderUpdateModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }

        public bool IsVisible { get; set; }
        public bool IsDisabled { get; set; }
        public string DisableMessage { get; set; }

        public ImageModel Avatar { get; set; }
        public ImageModel Background { get; set; }

        public string Category { get; set; }
        public List<string> Tags { get; set; }

        public List<ProviderTranslationUpdateModel> Translations { get; set; }
        public List<ProviderSocialUpdateModel> Socials { get; set; } = new List<ProviderSocialUpdateModel>();
    }

    public class ProviderTranslationUpdateModel
    {
        public string LanguageCode { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string KeyWords { get; set; }
        public bool IsDefault { get; set; }
    }

    public class ProviderSocialUpdateModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Secret { get; set; }
        public ProviderSocialType Type { get; set; }
    }
}
