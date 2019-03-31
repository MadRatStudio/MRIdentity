using GoogleClient.Utilites;

namespace GoogleClient.Nodes.Base
{
    public abstract class BaseUserNode
    {
        protected readonly GoogleUserClient _client;
        protected readonly UserRequestSender _userRequestSender = new UserRequestSender();

        public BaseUserNode(GoogleUserClient client)
        {
            _client = client;
        }
    }
}
