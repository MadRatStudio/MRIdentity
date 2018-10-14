﻿using Dal;
using Infrastructure.Entities;
using Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MRDbIdentity.Domain;
using MRDbIdentity.Infrastructure.Interface;
using MRDbIdentity.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApi.Init
{
    public static class DI
    {
        public static void AddDependencies(IServiceCollection services, IConfiguration configuration)
        {
            // Identity Services
            services.AddTransient<IUserStore<AppUser>, UserRepository<AppUser>>();
            services.AddTransient<IRoleStore<Role>, RoleRepository>();
            services.AddTransient<IUserRepository<AppUser>, UserRepository<AppUser>>();
            services.AddTransient<IRoleRepository, RoleRepository>();
            services.AddTransient<SignInManager<User>>();
            services.AddTransient(x => AppUserManager.Create(new MongoClient(configuration["ConnectionStrings:Default"]).GetDatabase(configuration["Database:Name"])));
            services.AddTransient(x => new MongoClient(configuration["ConnectionStrings:Default"]).GetDatabase(configuration["Database:Name"]));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

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
        }
    }
}
