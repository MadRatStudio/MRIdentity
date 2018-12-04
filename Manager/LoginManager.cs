using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using CommonApi.Errors;
using CommonApi.Identity;
using CommonApi.Models;
using CommonApi.Resopnse;
using CommonApi.Response;
using Dal;
using Infrastructure.Entities;
using Infrastructure.Model.Provider;
using Infrastructure.Model.User;
using Infrastructure.System.Options;
using Manager.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace Manager
{
    public class LoginManager : BaseManager
    {
        protected readonly AppUserRepository _appUserRepository;
        protected readonly ProviderRepository _providerRepository;

        protected readonly Regex QMARK_REGEX = new Regex("[?]");

        public LoginManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager, IMapper mapper, ILoggerFactory loggerFactory,
            AppUserRepository appUserRepository, ProviderRepository providerRepository) : base(httpContextAccessor, appUserManager, mapper, loggerFactory)
        {
            _appUserRepository = appUserRepository;
            _providerRepository = providerRepository;
        }

        /// <summary>
        /// Login with email model to provider
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="model">UserProviderEmailLogin model</param>
        /// <returns></returns>
        public async Task<ApiResponse<ProviderTokenResponse>> ProviderLoginEmail(HttpContext context, UserProviderEmailLogin model)
        {
            var user = await _appUserManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _appUserManager.CheckPasswordAsync(user, model.Password))) return Fail(ECollection.USER_NOT_FOUND);

            var provider = await _providerRepository.GetFirst(x => x.Id == model.ProviderId && x.State);
            if (provider == null)
                return Fail(ECollection.PROVIDER_NOT_FOUND);

            if (!provider.IsLoginEnabled)
                return Fail(ECollection.PROVIDER_UNAVALIABLE);

            var response = new ProviderTokenResponse
            {
                Token = _createShortLiveToken(user, provider)
            };

            response.RedirectUrl = _createRedirectUrl(provider, response.Token);

            return Ok(response);
        }

        /// <summary>
        /// Instant login to provider
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="providerId">Provider id</param>
        /// <returns></returns>
        public async Task<ApiResponse<ProviderTokenResponse>> ProviderLoginInstant(HttpContext context, string providerId)
        {
            var user = await GetCurrentUser();
            if (user == null)
                return Fail(ECollection.NOT_AUTHORIZED);

            var provider = await _providerRepository.GetFirst(x => x.Id == providerId && x.State);
            if (provider == null)
                return Fail(ECollection.PROVIDER_NOT_FOUND);

            if (!provider.IsLoginEnabled)
                return Fail(ECollection.PROVIDER_UNAVALIABLE);

            var response = new ProviderTokenResponse
            {
                Token = _createShortLiveToken(user, provider)
            };

            response.RedirectUrl = _createRedirectUrl(provider, response.Token);

            return Ok(response);
        }

        /// <summary>
        /// Login approve logic
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="token">User`s short live token</param>
        /// <returns></returns>
        public async Task<ApiResponse<MRLoginResponseModel>> ProviderApproveLogin(HttpContext context, string token, string fingerprint)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Fail(ECollection.UNSUPPORTED_REQUEST);

            var challengeResult = await _challengeShortLiveToken(token, fingerprint);
            if (!challengeResult.IsSuccess)
            {
                return Fail(challengeResult.Error);
            }

            var user = await _appUserRepository.GetFirst(challengeResult.UserId);
            if(user == null)
                return Fail(ECollection.USER_NOT_FOUND);

            if (user.IsBlocked)
                return Fail(ECollection.USER_BLOCKED);

            var targetUProvider = user.ConnectedProviders.FirstOrDefault(x => x.ProviderId == challengeResult.ProviderId);
            if(targetUProvider == null)
            {
                targetUProvider = new AppUserProvider
                {
                    ProviderId = challengeResult.ProviderId,
                    ProviderName = challengeResult.Provider.Name,
                    Roles = challengeResult.Provider.Roles?.Where(x => x.IsDefault).ToList() ?? new List<ProviderRole>(),
                    UpdatedTime = DateTime.UtcNow,
                    Metadata = new List<AppUserProviderMeta>
                    {
                        _createMeta(context)
                    }
                };

                await _appUserRepository.AddProvider(user.Id, targetUProvider);
            }
            else
            {
                await _appUserRepository.AddProviderMeta(user.Id, targetUProvider.ProviderId, _createMeta(context));
            }

            var result = new MRLoginResponseModel
            {
                AvatarSrc = user.Avatar?.Src,
                Email = user.Email,
                Id = user.Id,
                Roles = targetUProvider.Roles.Select(x => x.Name).ToList(),
                Tel = user.Tels?.FirstOrDefault()?.Number,
            };

            return Ok(result);
        }

        /// <summary>
        /// Create meta
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected AppUserProviderMeta _createMeta(HttpContext context)
        {
            var userAgent = "Unknown agent";
            Microsoft.Extensions.Primitives.StringValues agent = new Microsoft.Extensions.Primitives.StringValues();
            if (context.Request.Headers.TryGetValue("User-Agent", out agent))
            {
                userAgent = agent.FirstOrDefault() ?? "Unknown agent";
            }

            return new AppUserProviderMeta
            {
                Ip = context.Connection.RemoteIpAddress.ToString(),
                UserAgent = userAgent,
                UpdatedTime = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Create short live token for chalange
        /// </summary>
        /// <param name="user">Target user</param>
        /// <param name="provider">Target provider</param>
        /// <returns>Short live token</returns>
        protected string _createShortLiveToken(AppUser user, Provider provider)
        {
            var claims = new List<Claim>
            {
                new Claim(MRClaims.ID, user.Id),
                new Claim(MRClaims.EMAIL, user.Email),
                new Claim(MRClaims.PROVIDER_ID, provider.Id),
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, "Token", MRClaims.EMAIL, MRClaims.PROVIDER_ID);

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(5);

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                expires: expires,
                claims: identity.Claims,
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));


            var encoded = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encoded;
        }
        
        /// <summary>
        /// Chalange short live token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="providerFingerprint"></param>
        /// <returns></returns>
        protected async Task<ShortLiveTokenChalangeResult> _challengeShortLiveToken(string token, string providerFingerprint)
        {
            var response = new ShortLiveTokenChalangeResult();

            var provider = await _providerRepository.GetByFingerprint(providerFingerprint);
            if(provider == null)
            {
                response.Error = ECollection.Select(ECollection.TOKEN_PROVIDER_NOT_FOUND);
                return response;
            }

            if (!provider.IsLoginEnabled)
            {
                response.Error = ECollection.TOKEN_PROVIDER_NOT_ALLOWED;
                return response;
            }

            var validator = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.FromMinutes(5),
                IssuerSigningKeys = new List<SecurityKey> { AuthOptions.GetSymmetricSecurityKey() },
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidAudience = AuthOptions.AUDIENCE,
                ValidIssuer = AuthOptions.ISSUER,
                ValidateIssuer = true
            };

            try
            {
                var claimsPrincipal = new JwtSecurityTokenHandler()
                    .ValidateToken(token, validator, out var rawValidatedToken);

                var securityToken = (JwtSecurityToken)rawValidatedToken;

                response.UserId = securityToken.Claims.FirstOrDefault(x => x.Type == MRClaims.ID)?.Value;
                response.UserEmail = securityToken.Claims.FirstOrDefault(x => x.Type == MRClaims.EMAIL)?.Value;
                response.ProviderId = securityToken.Claims.FirstOrDefault(x => x.Type == MRClaims.PROVIDER_ID)?.Value;

                if (string.IsNullOrWhiteSpace(response.UserId) || string.IsNullOrWhiteSpace(response.UserEmail) || string.IsNullOrWhiteSpace(response.ProviderId))
                    throw new Exception("Damaged access token");
            }
            catch (SecurityTokenValidationException stvex)
            {
                // The token failsed validation!
                // TODO: Log it or display an error.
                throw new Exception($"Token failed validation: {stvex.Message}");
            }
            catch (Exception ex)
            {
                // TODO add logs here
                response.Error = ECollection.TOKEN_CHALLENGE_FAILED;
                return response;
            }

            if(provider.Id != response.ProviderId)
            {
                response.Error = ECollection.ACCESS_DENIED;
                return response;
            }

            response.Provider = provider;


            return response;
        }

        /// <summary>
        /// Create url for user redirect
        /// </summary>
        /// <param name="provider">Target provider</param>
        /// <param name="token">Short live access token</param>
        /// <returns></returns>
        protected string _createRedirectUrl(Provider provider, string token)
        {
            var url = provider.LoginRedirectUrl;
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;

            url = url.Trim(new char[] { ' ', '/' , '?', '&'});
            url += QMARK_REGEX.IsMatch(url) ? "&" : "?";

            return url + ProviderOptions.LOGIN_PARAM_NAME + "=" + token;
        }

        /// <summary>
        /// Short live token chalange result
        /// </summary>
        protected class ShortLiveTokenChalangeResult
        {
            public bool IsSuccess => Error == null;
            public ApiError Error { get; set; }
            public string UserId { get; set; }
            public string UserEmail { get; set; }
            public string ProviderId { get; set; }
            public Provider Provider { get; set; }
        }
    }
}
