namespace GoogleClient.Utilites
{
    public class Navigator
    {
        const string BASE_URL = "https://www.googleapis.com/oauth2";

        protected string Version { get; set; }

        protected string fullUrl => $"{BASE_URL}/{Version}";

        public Navigator() { }
        public Navigator(string version)
        {
            Version = version;
        }

        #region profile

        public string Profile() => $"{fullUrl}/userinfo?alt=json";

        #endregion
    }
}
