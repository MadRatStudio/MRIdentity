using Hangfire;
using Hangfire.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service;
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
            var provider = services.BuildServiceProvider();

            var backgroundJob = provider.GetRequiredService<IBackgroundJobClient>();

            var emailService = provider.GetRequiredService<EmailMadRatBotService>();

            backgroundJob
                .Schedule(() => emailService.SendEmails().Wait(), emailService.Schedule);
        }
    }
}
