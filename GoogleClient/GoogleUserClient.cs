using GoogleClient.Nodes.User;
using GoogleClient.Utilites;
using System;

namespace GoogleClient
{
    public class GoogleUserClient
    {
        public string Token { get; set; }
        public Navigator Navigator { get; set; }

        #region nodes

        public readonly UserNode User;

        #endregion

        public GoogleUserClient() : this(string.Empty) { }
        public GoogleUserClient(string token, string version = "v1")
        {
            Token = token;
            Navigator = new Navigator(version);

            User = new UserNode(this);
        }

        public GoogleUserClient SetToken(string token)
        {
            Token = token;
            return this;
        }
        public GoogleUserClient SetVersion(string version)
        {
            Navigator = new Navigator(version);
            return this;
        }


    }
}
