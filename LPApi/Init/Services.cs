using Hangfire;
using Hangfire.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service;

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
            var reccuringJob = provider.GetRequiredService<IRecurringJobManager>();
            var emailService = provider.GetRequiredService<EmailMadRatBotService>();

            RecurringJob.AddOrUpdate("service-email-common", () => emailService.SendEmailsSync(), Cron.Minutely);
        }
    }
}
