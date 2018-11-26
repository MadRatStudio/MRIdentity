using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Dal;
using Microsoft.AspNetCore.Http;

namespace Manager
{
    public class ProviderUserManager : BaseManager
    {
        public ProviderUserManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper) : base(httpContextAccessor, appUserManager, mapper)
        {

        }
    }
}
