using Amazon;
using Dal;
using Infrastructure.Entities;
using Infrastructure.System.Appsettings;
using Infrastructure.System.Options;
using Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MRDbIdentity.Domain;
using MRDbIdentity.Infrastructure.Interface;
using MRDbIdentity.Service;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools;

namespace IdentityApi.Init
{
    public static class DI
    {
        public static void AddDependencies(IServiceCollection services, IConfiguration configuration)
        {

            // options
            TemplateSettings tSettings = new TemplateSettings();
            configuration.GetSection("Templates").Bind(tSettings);
            services.AddSingleton(tSettings);

            ExternalServiceSettings externalSettings = new ExternalServiceSettings();
            configuration.GetSection("ExternalServices").Bind(externalSettings);
            services.AddSingleton(externalSettings);

            UrlRedirectSettings urlSettings = new UrlRedirectSettings();
            configuration.GetSection("UrlRedirect").Bind(urlSettings);
            services.AddSingleton(urlSettings);

            // tools
            services.AddTransient<TemplateParser>();

            // Identity Services
            services.AddTransient<IUserStore<AppUser>, UserRepository<AppUser>>();
            services.AddTransient<IRoleStore<Role>, RoleRepository>();
            services.AddTransient<IUserRepository<AppUser>, UserRepository<AppUser>>();
            services.AddTransient<IRoleRepository, RoleRepository>();
            services.AddTransient<AppUserRepository>();
            services.AddTransient<SignInManager<User>>();
            services.AddTransient(x => AppUserManager.Create(new MongoClient(configuration["ConnectionStrings:Default"]).GetDatabase(configuration["Database:Name"])));
            services.AddTransient(x => new MongoClient(configuration["ConnectionStrings:Default"]).GetDatabase(configuration["Database:Name"]));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<ImageTmpBucket>(x => (ImageTmpBucket)new ImageTmpBucket(RegionEndpoint.USEast1, "AKIAJJKBZQCLBYWOJX5A", "I0xyr6J2mPQaiENC1s16MTHbgek7A9i8ES1mdF16").SetBucket("madrat-media").SetSubdirectory("img_tmp"));
            services.AddTransient<ImageOriginBucket>(x => (ImageOriginBucket)new ImageOriginBucket(RegionEndpoint.USEast1, "AKIAJJKBZQCLBYWOJX5A", "I0xyr6J2mPQaiENC1s16MTHbgek7A9i8ES1mdF16").SetBucket("madrat-media").SetSubdirectory("img_origin"));

            Type[] ignoreRepos = new Type[] { typeof(AppUserManager), typeof(ImageTmpBucket), typeof(ImageOriginBucket) };

            // repos
            services.Scan(x =>
                x.FromAssemblyOf<ProviderRepository>()
                .AddClasses(c => c.Where(z => !ignoreRepos.Contains(z)))
                .AsSelf().WithTransientLifetime());

            //managers
            services.Scan(x => x
                .FromAssemblyOf<AccountManager>()
                .AddClasses(true)
                .AsSelf()
                .WithTransientLifetime());

            // services
            services.Scan(x =>
                x.FromAssemblyOf<EmailMadRatBotService>()
                .AddClasses(c => c.Where(z => !ignoreRepos.Contains(z)))
                .AsSelf().WithTransientLifetime());

            // email settings
            services.Configure<EmailConfigurationMadRatBot>(configuration.GetSection("Email").GetSection("MadRatBot"));

            /*
            services.AddTransient<LanguageRepository>();
            services.AddTransient<ProviderRepository>();
            services.AddTransient<ProviderCategoryRepository>();
            services.AddTransient<ProviderTagRepository>();


            // managers
            services.AddTransient<AccountManager>();
            services.AddTransient<UserManager>();
            services.AddTransient<LanguageManager>();
            services.AddTransient<TagManager>();
            services.AddTransient<CategoryManager>();
            services.AddTransient<ProviderManager>();
            services.AddTransient<ImageManager>();
            services.AddTransient<LoginManager>();
            */
        }
    }
}
