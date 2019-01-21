using System.Collections.Generic;

namespace CommonApi.Models
{
    public class MRLoginResponseModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Tel { get; set; }
        public string AvatarSrc { get; set; }
        public List<string> Roles { get; set; }
    }
}
