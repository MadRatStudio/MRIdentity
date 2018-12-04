using MRDb.Domain;
using MRDb.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Entities
{
    public class UserInvite : Entity, IEntity
    {
        public string UserId { get; set; }

        public bool IsByIdentity { get; set; }

        public string ProviderId { get; set; }
        public string ProviderName { get; set; }

        public string Code { get; set; }
    }
}
