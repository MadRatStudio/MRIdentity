using MRDb.Domain;
using MRDb.Infrastructure.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Entities
{
    public class Provider : Entity, IEntity
    {
        public string Name { get; set; }
        public string Slug { get; set; }

        public ProviderOwner Owner { get; set; }

        public bool IsVisible { get; set; }

        public bool IsDisabled { get; set; }
        public string DisableMessage { get; set; }

        // server info

        public string HomePage { get; set; }
        public string LoginRedirectUrl { get; set; }
        public List<ProviderFingerprint> Fingerprints { get; set; }

        // end server info

        public List<ProviderTranslation> Translations { get; set; }
        public List<ProviderSocial> Socials { get; set; }

        public Image Background { get; set; }
        public Image Avatar { get; set; }

        public ProviderProviderCategory Category { get; set; }
        public List<ProviderProviderTag> Tags { get; set; }
    }

    public class ProviderFingerprint
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        public string Fingerprint { get; set; }
        public DateTime FingerprintUpdateTime { get; set; }
    }

    public class ProviderOwner
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class ProviderTranslation
    {
        public string LanguageCode { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string KeyWords { get; set; }
        public bool IsDefault { get; set; }
    }

    public class ProviderSocial
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Secret { get; set; }
        public ProviderSocialType Type { get; set; }
    }

    public class ProviderProviderCategory
    {
        public string CategoryId { get; set; }
        public string Slug { get; set; }
    }

    public class ProviderProviderTag
    {
        public string TagId { get; set; }
        public string Key { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProviderSocialType
    {
        Facebook,
        Google,
        Twitter
    }
}
