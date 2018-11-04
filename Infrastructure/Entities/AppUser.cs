﻿using MRDb.Domain;
using MRDb.Infrastructure.Interface;
using MRDbIdentity.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Entities
{
    public class AppUser : User, IEntity
    {
        public List<UserSocial> Socials { get; set; } = new List<UserSocial>();
        public List<AppUserProvider> ConnectedProviders { get; set; } = new List<AppUserProvider>();
    }

    public class UserSocial
    {
        public string Name { get; set; }
        public string Token { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class AppUserProvider
    {
        public string ProviderId { get; set; }
        public string ProviderName { get; set; }
        public DateTime UpdatedTime { get; set; }
        public List<AppUserProviderMeta> Metadata { get; set; } = new List<AppUserProviderMeta>();
    }

    public class AppUserProviderMeta
    {
        public string Ip { get; set; }
        public string UserAgent { get; set; }
        public DateTime UpdatedTime { get; set; }
    }
}
