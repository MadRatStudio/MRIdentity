using MRDb.Domain;
using MRDb.Infrastructure.Interface;

namespace Infrastructure.Entities
{
    public class UserResetPassword : Entity, IEntity
    {
        public string UserId { get; set; }
        public string Code { get; set; }
    }
}
