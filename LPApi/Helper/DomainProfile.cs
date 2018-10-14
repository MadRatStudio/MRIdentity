using AutoMapper;
using Infrastructure.Entities;
using Infrastructure.Model.Common;
using Infrastructure.Model.Provider;
using Infrastructure.Model.User;

namespace IdentityApi.Helper
{
    public class DomainProfile : Profile
    {
        public DomainProfile()
        {
            CreateMap<AppUser, UserLoginResponseModel>()
                .ForMember(x => x.ImageSrc, opt => opt.ResolveUsing(x => x.Avatar?.Src));

            // common models
            CreateMap<Language, LanguageDisplayModel>();

            // provider models
            CreateMap<ProviderCategory, ProviderCategoryDisplayModel>();
            CreateMap<CategoryUpdateModel, ProviderCategory>();
            CreateMap<CategoryTranslationUpdateModel, ProviderCategoryTranslation>();

            // provider tag
            CreateMap<ProviderTag, ProviderTagDisplayModel>();
            CreateMap<ProviderTagCreateModel, ProviderTag>();
            CreateMap<ProviderTagTranslationCreateModel, ProviderTagTranslation>();


            CreateMap<Provider, ProviderShortDisplayModel>();
            CreateMap<Provider, ProviderDisplayModel>()
                .IncludeBase<Provider, ProviderShortDisplayModel>();

        }
    }
}
