using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Model.Provider
{
    public class CategoryUpdateModel
    {
        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("translations")]
        public List<CategoryTranslationUpdateModel> Translations { get; set; }
    }

    public class CategoryTranslationUpdateModel
    {
        public string LanguageCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
    }
}
