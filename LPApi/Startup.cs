using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MRDbIdentity.Domain;
using Microsoft.IdentityModel.Tokens;
using Manager.Options;
using AutoMapper;
using IdentityApi.Init;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.IO;
using System;
using Hangfire;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Infrastructure.System.Options;
using Hangfire.Dashboard;
using Microsoft.Extensions.Logging;
using NLog.Web;
using NLog.Extensions.Logging;
using IdentityApi.Middleware;

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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.AddNLog();
            env.ConfigureNLog("nlog.config");

            app.UseDefaultFiles();
            app.UseAuthentication();

            app.UseHangfireDashboard(Configuration["Hangfire:DashboardUrl"], new DashboardOptions
            {
                AppPath = Configuration["Client:Admin"],
                Authorization = new List<IDashboardAuthorizationFilter> { new MRDashboardAuthorizationFilter() }
            });
            app.UseHangfireServer();

            app.UseStaticFiles();
            app.UseCors("AllowAll");
            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/swagger/v1/swagger.json", "Auto generated Identity Api");
            });
        }

        protected void ConfigureHangfireDashboard()
        {

        }
    }

    public class MRDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([Hangfire.Annotations.NotNull] DashboardContext context)
        {

            var httpContext = context.GetHttpContext();
            var referer = httpContext.Request.Headers["Referer"].FirstOrDefault();
            if (referer != null && referer.Contains("/hangfire")) return true;

            if (!httpContext.Request.PathBase.HasValue || !httpContext.Request.PathBase.Value.Contains("/hangfire")) return false;
            if (!httpContext.Request.Query.ContainsKey("tkn")) return false;

            var token =  httpContext.Request.Query["tkn"];
            if (string.IsNullOrWhiteSpace(token)) return false;

            var validationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.FromMinutes(5),
                // Specify the key used to sign the token:
                IssuerSigningKeys = new List<SecurityKey> { AuthOptions.GetSymmetricSecurityKey() },
                RequireSignedTokens = true,
                // Ensure the token hasn't expired:
                RequireExpirationTime = true,
                ValidateLifetime = true,
                // Ensure the token audience matches our audience value (default true):
                ValidateAudience = true,
                ValidAudience = AuthOptions.AUDIENCE,
                // Ensure the token was issued by a trusted authorization server (default true):
                ValidateIssuer = true,
                ValidIssuer = AuthOptions.ISSUER
            };

            try
            {
                var claimsPrincipal = new JwtSecurityTokenHandler()
                    .ValidateToken(token, validationParameters, out var rawValidatedToken);

                var securityToken = (JwtSecurityToken)rawValidatedToken;
                var roles = securityToken.Claims.Where(x => x.Type == ClaimsIdentity.DefaultRoleClaimType);

                if (roles == null || !roles.Any() || !roles.Any(x => x.Value == AppUserRoleList.ADMIN)) return false;
                return true;
            }
            catch (SecurityTokenValidationException stvex)
            {
                // The token failed validation!
                // TODO: Log it or display an error.
                throw new Exception($"Token failed validation: {stvex.Message}");
            }
            catch (ArgumentException argex)
            {
                // The token was not well-formed or was invalid for some other reason.
                // TODO: Log it or display an error.
                throw new Exception($"Token was invalid: {argex.Message}");
            }
        }
    }
}
