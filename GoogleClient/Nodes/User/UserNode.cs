using GoogleClient.Infrastructure.Common;
using GoogleClient.Infrastructure.Request;
using GoogleClient.Infrastructure.User.Profile;
using GoogleClient.Nodes.Base;
using System.Threading.Tasks;

namespace GoogleClient.Nodes.User
{
    public class UserNode : BaseUserNode
    {
        public UserNode(GoogleUserClient client) : base(client) { }

        public async Task<ResponseModel<ProfileDisplayModel>> Profile()
        {
            var request = new RequestModel(_client.Token, _client.Navigator.Profile());
            return await _userRequestSender.Get<ProfileDisplayModel>(request);
        }
    }
}
