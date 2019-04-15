using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Dal;
using Dal.Tasks;
using GoogleClient;
using Infrastructure.Entities;
using Infrastructure.Model.Provider;
using Infrastructure.Model.User;
using Infrastructure.System.Appsettings;
using Infrastructure.System.Options;
using Infrastructure.Template.User;
using Manager.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MRIdentityClient.Email;
using MRIdentityClient.Exception.Common;
using MRIdentityClient.Exception.MRSystem;
using MRIdentityClient.Exception.Provider;
using MRIdentityClient.Exception.Request;
using MRIdentityClient.Exception.User;
using MRIdentityClient.Models.User;
using MRIdentityClient.Response;
using Tools;
using TokenOptions = Manager.Options.TokenOptions;

namespace Manager
{
    public class LoginManager : BaseManager
    {
        protected readonly AppUserRepository _appUserRepository;
        protected readonly ProviderRepository _providerRepository;
        protected readonly EmailSendTaskRepository _emailSendTaskRepository;
        protected readonly UrlRedirectSettings _urlRedirectSettings;
        protected readonly TemplateParser _templateParser;
        protected readonly UserResetPasswordRepository _userResetPasswordRepository;
        protected readonly ExternalServiceSettings _externalServiceSettings;

        protected readonly Regex QMARK_REGEX = new Regex("[?]");

        public LoginManager(IHttpContextAccessor httpContextAccessor, AppUserManager appUserManager,
            IMapper mapper, ILoggerFactory loggerFactory,
            AppUserRepository appUserRepository, ProviderRepository providerRepository,
            EmailSendTaskRepository emailSendTaskRepository, UrlRedirectSettings urlRedirectSettings,
            TemplateParser templateParser, UserResetPasswordRepository userResetPasswordRepository, ExternalServiceSettings externalServiceSettings) : base(httpContextAccessor, appUserManager, mapper, loggerFactory)
        {
            _appUserRepository = appUserRepository;
            _providerRepository = providerRepository;
            _emailSendTaskRepository = emailSendTaskRepository;
            _urlRedirectSettings = urlRedirectSettings;
            _templateParser = templateParser;
            _userResetPasswordRepository = userResetPasswordRepository;
            _externalServiceSettings = externalServiceSettings;
        }

        #region login

        /// <summary>
        /// Generate user token by email
        /// </summary>
        /// <param name="model">Sign in model</param>
        /// <returns>User token model</returns>
        public async Task<UserLoginResponseModel> LoginEmail(UserLoginModel model)
        {
            if (model == null)
                throw new BadRequestException();

            var user = await _appUserManager.FindByEmailAsync(model.Email);
            if (user == null)
                throw new LoginUserNotFound(model.Email);

            var checkPasswordResult = await _appUserManager.CheckPasswordAsync(user, model.Password);
            if (!checkPasswordResult)
                throw new LoginFailedException(model.Email);

            return await AuthUserWithToken(user);
        }

        public async Task<UserLoginResponseModel> LoginGoogle(UserGoogleAuthModel model)
        {
            var googleClient = new GoogleUserClient(model.Data);
            var response = await googleClient.User.Profile();

            if (!response.IsSuccess || response.Data == null)
            {
                throw new ExternalLoginException();
            }

            var data = response.Data;

            var user = await _appUserManager.FindByEmailAsync(data.Email);
            if (user == null)
            {
                user = new AppUser
                {
                    Email = data.Email,
                    FirstName = data.GivenName,
                    LastName = data.FamilyName,
                    UserName = data.Email,
                    Avatar = new MRDbIdentity.Domain.UserAvatar
                    {
                        Src = data.Picture
                    },
                    Socials = new List<UserSocial>
                    {
                        new UserSocial
                        {
                            CreatedTime = DateTime.UtcNow,
                            Name = "GOOGLE",
                            Token = model.Data
                        }
                    }
                };

                var userCreate = await _appUserManager.CreateAsync(user);
                if (!userCreate.Succeeded)
                {
                    throw new ExternalLoginException();
                }

                var addRole = await _appUserManager.AddToRoleAsync(user, "USER");
                if (!addRole.Succeeded)
                {
                    throw new ExternalLoginException();
                }
            }
            else
            {
                if (user.Socials == null)
                    user.Socials = new List<UserSocial>();

                var social = user.Socials.FirstOrDefault(x => x.Name == "GOOGLE");
                if (social == null)
                {
                    user.Socials.Add(new UserSocial
                    {
                        CreatedTime = DateTime.UtcNow,
                        Token = model.Data,
                        Name = "GOOGLE"
                    });
                }
                else
                {
                    social.CreatedTime = DateTime.UtcNow;
                    social.Token = model.Data;
                }

                await _appUserManager.UpdateAsync(user);
            }

            return await AuthUserWithToken(user);
        }

        #endregion

        #region Signup

        public async Task<UserLoginResponseModel> SignupEmail(UserSignupModel model)
        {
            if (model == null)
                throw new ModelDamagedException("Model is required");

            var user = await _appUserManager.FindByEmailAsync(model.Email);
            if (user != null)
                throw new MRSystemException("Email already used");

            user = _mapper.Map<AppUser>(model);

            var userCreateResult = await _appUserManager.CreateAsync(user, model.Password);
            if (!userCreateResult.Succeeded)
                throw new MRSystemException("Can not create user");

            return await AuthUserWithToken(user);
        }

        #endregion


        /// <summary>
        /// Login with email model to provider
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="model">UserProviderEmailLogin model</param>
        /// <returns></returns>
        public async Task<ProviderTokenResponse> ProviderLoginEmail(HttpContext context, UserProviderEmailLogin model)
        {
            var user = await _appUserManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _appUserManager.CheckPasswordAsync(user, model.Password)))
                throw new LoginFailedException(model.Email);

            var provider = await _providerRepository.GetFirst(x => x.Id == model.ProviderId && x.State);
            if (provider == null)
                throw new EntityNotFoundException(model.ProviderId, typeof(Provider));

            if (!provider.IsLoginEnabled)
                throw new ProviderUnavaliableException(provider.Name);

            var response = new ProviderTokenResponse
            {
                Token = _createShortLiveToken(user, provider)
            };

            response.RedirectUrl = _createRedirectUrl(provider, response.Token);

            return response;
        }

        /// <summary>
        /// Instant login to provider
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="providerId">Provider id</param>
        /// <returns></returns>
        public async Task<ProviderTokenResponse> ProviderLoginInstant(HttpContext context, string providerId)
        {
            var user = await GetCurrentUser();
            if (user == null)
                throw new AccessDeniedException(string.Empty, typeof(AppUser), "Authorization required");

            var provider = await _providerRepository.GetFirst(x => x.Id == providerId && x.State);
            if (provider == null)
                throw new MRSystemException("Provider not found");

            if (!provider.IsLoginEnabled)
                throw new ProviderUnavaliableException(provider.Name);

            var response = new ProviderTokenResponse
            {
                Token = _createShortLiveToken(user, provider)
            };

            response.RedirectUrl = _createRedirectUrl(provider, response.Token);

            return response;
        }

        /// <summary>
        /// Login approve logic
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="token">User`s short live token</param>
        /// <returns></returns>
        public async Task<ApproveLogin> ProviderApproveLogin(HttpContext context, string token, string fingerprint)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new MRSystemException();

            var challengeResult = await _challengeShortLiveToken(token, fingerprint);
            if (!challengeResult.IsSuccess)
                throw new MRSystemException(challengeResult.Error.Message);

            var user = await _appUserRepository.GetFirst(challengeResult.UserId);
            if (user == null)
                throw new EntityNotFoundException(challengeResult.UserId, typeof(AppUser));

            if (user.IsBlocked)
                throw new MRSystemException("User blocked");

            var targetUProvider = user.ConnectedProviders.FirstOrDefault(x => x.ProviderId == challengeResult.ProviderId);
            if (targetUProvider == null)
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

            var result = new ApproveLogin
            {
                AvatarUrl = user.Avatar?.Src,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Id = user.Id,
                Roles = targetUProvider.Roles.Select(x => x.Name).ToList(),
            };

            return result;
        }

        /// <summary>
        /// Request reset password token
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<ApiOkResult> ResetPasswordRequest(string email)
        {
            var user = await _appUserManager.FindByEmailAsync(email);
            if (user == null)
                throw new EntityNotFoundException(email, typeof(AppUser));

            var token = UserInviteCodeGenerator.Generate();

            var entity = new UserResetPassword
            {
                Code = token,
                UserId = user.Id
            };

            entity = await _userResetPasswordRepository.Insert(entity);

            var model = new ResetPasswordModel
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = token,
                Url = _urlRedirectSettings.ResetPasswordWithToken(user.Email, token)
            };

            var mailBody = await _templateParser.Render(EmailTemplateCollection.USER_RESET_PASSWORD, model);
            var to = user.Email;

            await _emailSendTaskRepository.InsertEmail(to, "Reset password", mailBody, Infrastructure.Entities.Tasks.EmailTaskBot.MadRatBot);

            return new ApiOkResult(true);
        }

        /// <summary>
        /// Approve password reset
        /// </summary>
        /// <param name="model">Password reset model</param>
        /// <returns></returns>
        public async Task<ApiOkResult> ResetPasswordApprove(UserResetPasswordModel model)
        {
            var user = await _appUserManager.FindByEmailAsync(model.Email);
            if (user == null)
                throw new EntityNotFoundException(model.Email, typeof(AppUser));

            var resetPasswordEntity = await _userResetPasswordRepository.GetByUser(user.Id, model.Token);
            if (resetPasswordEntity == null || (DateTime.UtcNow - resetPasswordEntity.CreatedTime).TotalHours > 24)
                throw new EntityNotFoundException(model.Token, typeof(UserResetPassword), "Can not find request to reset password");


            var result = await _appUserManager.RemovePasswordAsync(user);
            if (!result.Succeeded)
                throw new MRSystemException("Can not reset password");

            result = await _appUserManager.AddPasswordAsync(user, model.Password);
            if (!result.Succeeded)
                throw new MRSystemException("Can not reset password");

            await _userResetPasswordRepository.ResetAllCodes(user.Id);

            return new ApiOkResult(true);
        }

        /// <summary>
        /// Auth user and get token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected async Task<UserLoginResponseModel> AuthUserWithToken(AppUser user)
        {
            var userBucket = await GetIdentity(user);

            if (userBucket == null)
                throw new LoginFailedException(user.Email);

            var roles = userBucket.Item1;
            var identity = userBucket.Item2;

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

            await _appUserManager.AddLoginAsync(user, new UserLoginInfo(LoginOptions.SERVICE_LOGIN_PROVIDER, encoded, LoginOptions.SERVICE_LOGIN_DISPLAY));

            return response;
        }

        /// <summary>
        /// Add claims to user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected async Task<Tuple<List<string>, ClaimsIdentity>> GetIdentity(AppUser user)
        {
            if (user == null) return null;

            var roles = await _appUserManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
                new Claim(TokenOptions.USER_ID, user.Id)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, role));
            }

            ClaimsIdentity identity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            return new Tuple<List<string>, ClaimsIdentity>(roles.ToList(), identity);
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
            if (provider == null)
            {
                response.Error = new ApiError(-1, "Token provider not found");
                return response;
            }

            if (!provider.IsLoginEnabled)
            {
                response.Error = new ApiError(-1, "Token provider not allowed");
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
                response.Error = new ApiError(-1, "Token challenge failed");
                return response;
            }

            if (provider.Id != response.ProviderId)
            {
                response.Error = new ApiError(-1, "Access denied");
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

            url = url.Trim(new char[] { ' ', '/', '?', '&' });
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
