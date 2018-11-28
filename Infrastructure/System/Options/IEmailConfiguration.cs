namespace Infrastructure.System.Options
{
    public abstract class IEmailConfiguration
    {
        public string PrimaryDomain { get; set; }
        public int PrimaryPort { get; set; }
        public string SecondayDomain { get; set; }
        public int SecondaryPort { get; set; }
        public string UsernameEmail { get; set; }
        public string UsernameDisplay { get; set; }
        public string UsernamePassword { get; set; }
        public string FromEmail { get; set; }
    }

    public class EmailConfigurationMadRatBot : IEmailConfiguration { }
}
