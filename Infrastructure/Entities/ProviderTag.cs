using MRDb.Domain;
using MRDb.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Entities
{
    public class ProviderTag : Entity, IEntity
    {
        public string Key { get; set; }
        public List<ProviderTagTranslation> Translations { get; set; }

        public string UserCreatedId { get; set; }
    }

    public class ProviderTagTranslation
    {
        public string LanguageCode { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }
}
