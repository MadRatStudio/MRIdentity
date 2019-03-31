namespace Manager.Options
{
    public class UrlRedirectOptions
    {
        public string Root { get; set; }
        public string ResetPassword { get; set; }
        public string Invite { get; set; }
        public string Login { get; set; }
        public string Account { get; set; }

        public string ToLocal(string path) => $"{Root}{path}";
    }
}
