namespace GoogleClient.Infrastructure.Request
{
    public class RequestModel
    {
        public string Token { get; set; }
        public string Url { get; set; }

        public RequestModel() { }
        public RequestModel(string token, string url)
        {
            Token = token;
            Url = url;
        }
    }
}
