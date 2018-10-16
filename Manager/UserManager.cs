using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CommonApi.Resopnse;
using Dal;
using Infrastructure.Entities;
using Infrastructure.Model.User;
using Manager.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using MRDbIdentity.Infrastructure.Interface;
using MRDbIdentity.Service;

namespace Manager
{
    public class UserManager : BaseManager
    {
        public UserManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper) : base(httpContextAccessor, appUserManager, mapper)
        {
        }

        #region admin

        public async Task<ApiResponse> AdminGetCollection(int skip, int limit, string q)
        {

        }

        #endregion

        #region login

        /// <summary>
        /// Generate user token by email
        /// </summary>
        /// <param name="model">Sign in model</param>
        /// <returns>User token model</returns>
        public async Task<ApiResponse<UserLoginResponseModel>> TokenEmail(UserLoginModel model)
        {
            if (model == null) return Fail(0, null);
            var userBucket = await GetIdentity(model);

            if (userBucket == null) return Fail(0, "Bad login");

            var user = userBucket.Item1;
            var roles = userBucket.Item2;
            var identity = userBucket.Item3;

            var now = DateTime.UtcNow;
            var expires = now.Add(TimeSpan.FromSeconds(AuthOptions.LIFETIME));

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                expires: expires,
                claims: identity.Claims,
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var encoded = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = _mapper.Map<UserLoginResponseModel>(user);
            response.Roles = roles;
            response.Token = new UserLoginTokenResponseModel
            {
                Expires = expires,
                Token = encoded,
                LoginProvider = LoginOptions.SERVICE_LOGIN_PROVIDER,
                LoginProviderDisplay = LoginOptions.SERVICE_LOGIN_DISPLAY
            };

            await _appUserManager.AddLoginAsync(user, new Microsoft.AspNetCore.Identity.UserLoginInfo(LoginOptions.SERVICE_LOGIN_PROVIDER, encoded, LoginOptions.SERVICE_LOGIN_DISPLAY));

            return Ok(response);
        }

        #endregion


        /// <summary>
        /// Add claims to user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected async Task<Tuple<AppUser, List<string>, ClaimsIdentity>> GetIdentity(UserLoginModel model)
        {
            var user = await _appUserManager.FindByEmailAsync(model.Email);
            if (user == null) return null;

            if (!await _appUserManager.CheckPasswordAsync(user, model.Password)) return null;
            var roles = await _appUserManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
            };

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, role));
            }

            ClaimsIdentity identity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            return new Tuple<AppUser, List<string>, ClaimsIdentity>(user, roles.ToList(), identity);
        }
    }
}
