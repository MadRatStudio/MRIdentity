using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Infrastructure.System.Provider
{
    public class ModelRequiredProvider : IBindingMetadataProvider
    {
        public ModelRequiredProvider() { }

        public void CreateBindingMetadata(BindingMetadataProviderContext context) { }

        public void GetBindingMetadata(BindingMetadataProviderContext context)
        {
            if (context.PropertyAttributes.OfType<RequiredAttribute>().Any())
            {
                context.BindingMetadata.IsBindingRequired = true;
            }
        }
    }
}
