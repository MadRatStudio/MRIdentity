using Hangfire;
using Hangfire.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApi.Init
{
    public static class Services
    {
        public static void InitServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(x =>
            {
                x.UseMongoStorage(configuration["ConnectionStrings:Default"], configuration["Database:Hangfire"]);
            });


            ConfigurateServices(services, configuration);
        }

        private static void ConfigurateServices(IServiceCollection services, IConfiguration configuration)
        {

            var backgroundJob = services.BuildServiceProvider().GetRequiredService<IBackgroundJobClient>();
             
            backgroundJob.Schedule(() => Console.WriteLine("Hello"), TimeSpan.FromMinutes(1));
        }
    }
}
