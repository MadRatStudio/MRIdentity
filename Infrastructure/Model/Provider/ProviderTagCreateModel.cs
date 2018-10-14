using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Model.Provider
{
    public class ProviderTagCreateModel
    {
        public string Key { get; set; }
        public List<ProviderTagTranslationCreateModel> Translations { get; set; }
    }

    public class ProviderTagTranslationCreateModel
    {
        public string LanguageCode { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }
}
