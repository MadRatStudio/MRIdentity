using Dal;
using Manager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MRDbIdentity.Domain;
using MRDbIdentity.Infrastructure.Interface;
using MRDbIdentity.Service;
using Microsoft.IdentityModel.Tokens;
using Manager.Options;
using Microsoft.AspNetCore.Http;
using Infrastructure.Entities;
using AutoMapper.Execution;
using AutoMapper;
using IdentityApi.Init;
using Newtonsoft.Json.Serialization;
using Infrastructure.System.Provider;
using System.Reflection;
using System.IO;
using System;
using Hangfire;

namespace IdentityApi
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;

                    var parameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,

                        ValidAudience = AuthOptions.AUDIENCE,

                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true
                    };

#if DEBUG
                    parameters.ValidateAudience = false;
#else
                    parameters.ValidateAudience = true;
#endif

                    options.TokenValidationParameters = parameters;

                });

            services.AddIdentityCore<User>()
                .AddDefaultTokenProviders();

            DI.AddDependencies(services, Configuration);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = "Auto generated Identity Api",
                    Version = "v1",
                    Contact = new Swashbuckle.AspNetCore.Swagger.Contact
                    {
                        Email = "oleg.timofeev20@gmail.com",
                        Name = "Oleh Tymofieiev",
                        Url = "http://identity_dev.madrat.studio"
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.XML";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                c.IncludeXmlComments(xmlPath);
            });

            services.AddAutoMapper();
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials()));
            services.AddMvc(o =>
            {
                o.ModelMetadataDetailsProviders.Add(new ModelRequiredProvider());
            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-ddTH:mm:ss.Z";
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                options.SerializerSettings.ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                };
            });

            Services.InitServices(services, Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseAuthentication();

            app.UseHangfireDashboard();
            app.UseHangfireServer();

            app.UseStaticFiles();
            app.UseCors("AllowAll");
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/swagger/v1/swagger.json", "Auto generated Identity Api");
            });
        }
    }
}
