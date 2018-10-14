using Dal;
using System.Linq;
using Microsoft.AspNetCore.Http;
using MRDbIdentity.Infrastructure.Interface;
using MRDbIdentity.Service;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;
using MRDbIdentity.Domain;
using System.Threading.Tasks;
using Infrastructure.Entities;
using AutoMapper;
using CommonApi.Manager;

namespace Manager
{

    public class BaseManager : ApiResponseManager
    {
        protected readonly AppUserManager _appUserManager;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IMapper _mapper;

        protected string _currentUserEmail => _httpContextAccessor.HttpContext.User?.FindFirst(ClaimsIdentity.DefaultNameClaimType)?.Value;
        protected List<string> _currentUserRoles => _httpContextAccessor.HttpContext.User?.FindAll(ClaimsIdentity.DefaultRoleClaimType)?.Select(x => x.Value).ToList() ?? new List<string>();

        protected AppUser _currentUser { get; set; }
        protected async Task<AppUser> GetCurrentUser()
        {
            if (_currentUser == null)
            {
                var email = _currentUserEmail;
                if (string.IsNullOrWhiteSpace(email)) return null;

                _currentUser = await _appUserManager.FindByEmailAsync(email);
            }

            return _currentUser;
        }

        protected readonly string DEFAULT_LANGUAGE_CODE = "en";
        protected readonly int MAX_LIMIT = 15;

        public BaseManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _appUserManager = appUserManager;
            _mapper = mapper;
        }
    }
}
