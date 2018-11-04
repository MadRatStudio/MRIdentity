using Infrastructure.Entities;
using MongoDB.Driver;
using MRDbIdentity.Infrastructure.Interface;
using MRDbIdentity.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dal
{
    public class AppUserRepository : UserRepository<AppUser>
    {
        public AppUserRepository(IMongoDatabase mongoDatabase, IRoleRepository roleRepository) : base(mongoDatabase, roleRepository)
        {
        }

        public async Task<ICollection<AppUser>> Get(int skip, int limit, string q)
        {
            var query = DbQuery.Descending(x => x.CreatedTime);
            query.Skip = skip;
            query.Limit = limit;

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = $"*{q.ToLower()}*";
                query.CustomSearch(x =>
                    x.Or(
                        x.Regex(z => z.FirstName.ToLower(), q),
                        x.Regex(z => z.LastName.ToLower(), q),
                        x.Regex(z => z.Email.ToLower(), q)
                        ));
            }

            query.Projection(_shortUserProjection);

            return await Get(query);
        }

        public async Task<AppUser> GetByIdAdmin(string id)
        {
            var query = DbQuery.CustomSearch(x => x.Eq(z => z.Id, id));
            query.Projection(_fullUserProjection);

            return await GetFirst(query);
        }

        public async Task<AppUserProvider> GetProvider(string id, string providerId)
        {
            var q = DbQuery
                .CustomSearch(x => x.And(
                    x.Eq(z => z.Id, id),
                    x.ElemMatch(z => z.ConnectedProviders, z => z.ProviderId == providerId)))
                .Projection(x =>
                    x.Include(z => z.ConnectedProviders)
                    .ElemMatch(z => z.ConnectedProviders, z => z.ProviderId == providerId)
                    .Slice(z => z.ConnectedProviders, 0, 1));

            return (await Get(q))?.FirstOrDefault()?.ConnectedProviders?.FirstOrDefault();
        }

        public async Task<UpdateResult> AddProvider(string id, AppUserProvider provider)
        {
            var query = DbQuery.Eq(x => x.Id, id).Update(x => x.AddToSet(z => z.ConnectedProviders, provider));
            return await Update(query);
        }

        public async Task<UpdateResult> AddProviderMeta(string id, string providerId, AppUserProviderMeta meta)
        {
            var query = DbQuery.CustomSearch(x => x.And(
                x.Eq(z => z.Id, id),
                x.ElemMatch(z => z.ConnectedProviders, c => c.ProviderId == providerId)
                ))
                .Update(x => x.AddToSet(z => z
                    .ConnectedProviders[-1].Metadata, meta)
                    .Set(z => z.UpdatedTime, DateTime.UtcNow));

            return await Update(query);
        }

        protected Expression<Func<ProjectionDefinitionBuilder<AppUser>, ProjectionDefinition<AppUser>>> _shortUserProjection =>
            x => x
                .Include(z => z.Avatar)
                .Include(z => z.Birthday)
                .Include(z => z.CreatedTime)
                .Include(z => z.Email)
                .Include(z => z.FirstName)
                .Include(z => z.Id)
                .Include(z => z.IsBlocked)
                .Include(z => z.IsEmailConfirmed)
                .Include(z => z.LastName)
                .Include(z => z.UpdatedTime)
                .Include(z => z.UserName);

        protected Expression<Func<ProjectionDefinitionBuilder<AppUser>, ProjectionDefinition<AppUser>>> _fullUserProjection => x =>
            _shortUserProjection.Compile().Invoke(x)
            .Include(z => z.BlockedTime)
            .Include(z => z.BlockReason)
            .Include(z => z.BlockUntil)
            .Include(z => z.ConnectedProviders)
            .Include(z => z.Logins)
            .Include(z => z.Roles)
            .Include(z => z.Socials)
            .Include(z => z.Tels);
    }
}
