using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MRDbIdentity.Domain;
using MRDbIdentity.Service;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dal
{
    public class AppUserManager : UserManager<AppUser>
    {
        public AppUserManager(IUserStore<AppUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<AppUser> passwordHasher, IEnumerable<IUserValidator<AppUser>> userValidators, IEnumerable<IPasswordValidator<AppUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<AppUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public static AppUserManager Create(IMongoDatabase database)
        {
            var manager = new AppUserManager(new UserRepository<AppUser>(database, new RoleRepository(database)), null, new PasswordHasher<AppUser>(), null, null, new UpperInvariantLookupNormalizer(), null, null, new LoggerFactory().CreateLogger<AppUserManager>());
            return manager;
        }

        public Task AddToRolesAsync(KeyValuePair<string, User> user, object select)
        {
            throw new NotImplementedException();
        }
    }
}
