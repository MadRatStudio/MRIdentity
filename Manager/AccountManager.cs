using Dal;
using Infrastructure.Model.User;
using Microsoft.AspNetCore.Identity;
using MRDbIdentity.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Manager
{
    public class AccountManager
    {
        protected readonly AppUserManager _appUserManager;
        protected readonly SignInManager<User> _signInManager;

        public AccountManager(AppUserManager appUserManager, SignInManager<User> signInManager)
        {
            _appUserManager = appUserManager;
            _signInManager = signInManager;
        }

        public async Task SignIn(UserLoginModel model)
        {
            if (model == null) return;

            var user = await _appUserManager.FindByEmailAsync(model.Email);
            if (user == null) return;

            var isPassword = await _appUserManager.CheckPasswordAsync(user, model.Password);


            var s = isPassword;
        }
    }
}
