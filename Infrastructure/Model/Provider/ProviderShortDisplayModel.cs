using Infrastructure.Model.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Model.Provider
{
    public class ProviderShortDisplayModel
    {
        public string Id { get; set; }
        public bool IsLoginEnabled { get; set; }

        public string Name { get; set; }
        public string Slug { get; set; }

        public string HomePage { get; set; }

        public ImageModel Avatar { get; set; }

        public ProviderCategoryDisplayModel Category { get; set; }
        public List<ProviderTagDisplayModel> Tags { get; set; }
    }
}
