using MRDb.Domain;
using MRDb.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Entities
{
    public class ProviderCategory : Entity, IEntity
    {
        public string Slug { get; set; }
        public List<ProviderCategoryTranslation> Translations { get; set; }
    }

    public class ProviderCategoryTranslation
    {
        public string LanguageCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
    }
}
