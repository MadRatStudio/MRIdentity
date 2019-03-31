namespace GoogleClient.Infrastructure.User.Profile
{
    public class ProfileDisplayModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string VerifiedEmail { get; set; }
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string Link { get; set; }
        public string Picture { get; set; }
        public string Locale { get; set; }
    }
}
