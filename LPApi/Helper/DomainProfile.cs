using AutoMapper;
using Infrastructure.Entities;
using Infrastructure.Model.Common;
using Infrastructure.Model.Provider;
using Infrastructure.Model.User;
using MRDbIdentity.Domain;
using System;

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

            // user
            CreateMap<AppUser, UserShortDataModel>()
                .ForMember(x => x.AvatarSrc, opt => opt.ResolveUsing(z => z.Avatar?.Src))
                .ForMember(x => x.CreatedTime, opt => opt.ResolveUsing(z => z.CreatedTime.ToLocalTime()))
                .ForMember(x => x.UpdatedTime, opt => opt.ResolveUsing(z => z.UpdatedTime.ToLocalTime()));
            CreateMap<AppUser, UserDataModel>()
                .IncludeBase<AppUser, UserShortDataModel>();
            CreateMap<UserSocial, UserDataSocialModel>()
                .ForMember(x => x.CreatedTime, opt => opt.ResolveUsing(z => z.CreatedTime.ToLocalTime()));
            CreateMap<AppUserProvider, UserDataProviderModel>();
            CreateMap<UserTel, UserDataTel>()
                .ForMember(x => x.CreatedTime, opt => opt.ResolveUsing(z => z.CreatedTime.ToLocalTime()));


            // provider models
            CreateMap<ProviderCategory, ProviderCategoryDisplayModel>();
            CreateMap<CategoryUpdateModel, ProviderCategory>();
            CreateMap<CategoryTranslationUpdateModel, ProviderCategoryTranslation>();
            CreateMap<AppUser, ProviderOwner>();

            CreateMap<ProviderUpdateModel, Provider>()
                .ForMember(x => x.Tags, opt => opt.Ignore())
                .ForMember(x => x.Category, opt => opt.Ignore());
            CreateMap<ProviderTranslationUpdateModel, ProviderTranslation>();
            CreateMap<ProviderSocialUpdateModel, ProviderSocial>();

            CreateMap<Provider, ProviderUpdateModel>();
            CreateMap<ProviderTranslation, ProviderTranslationUpdateModel>();
            CreateMap<ProviderSocial, ProviderSocialUpdateModel>();


            // provider tag
            CreateMap<ProviderTag, ProviderTagDisplayModel>();
            CreateMap<ProviderTagCreateModel, ProviderTag>();
            CreateMap<ProviderTagTranslationCreateModel, ProviderTagTranslation>();


            CreateMap<Provider, ProviderShortDisplayModel>()
                .ForMember(x => x.Category, t => t.Ignore())
                .ForMember(x => x.Tags, t => t.Ignore());
            CreateMap<ProviderCategory, ProviderCategoryDisplayModel>();
            CreateMap<ProviderSocial, ProviderSocialDisplayModel>();
            CreateMap<ProviderTag, ProviderTagDisplayModel>();
            CreateMap<Provider, ProviderDisplayModel>()
                .IncludeBase<Provider, ProviderShortDisplayModel>();

        }
    }
}
