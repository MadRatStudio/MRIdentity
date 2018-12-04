using Infrastructure.Entities;
using MongoDB.Driver;
using MRDb.Infrastructure.Interface;
using MRDb.Repository;
using MRDb.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dal
{
    public class ProviderRepository : BaseRepository<Provider>, IRepository<Provider>
    {
        public ProviderRepository(IMongoDatabase mongoDatabase) : base(mongoDatabase) { }

        public async Task<Provider> GetShortBySlug(string slug)
        {
            var query = DbQuery
                .Eq(x => x.Slug, slug.ToLower())
                .Eq(x => x.State, true)
                .Projection(z => z
                    .Include(x => x.Id)
                    .Include(x => x.Name)
                    .Include(x => x.IsVisible)
                    .Include(x => x.IsLoginEnabled)
                    .Include(x => x.Slug)
                    .Include(x => x.UpdatedTime)
                    .Include(x => x.CreatedTime)
                    .Include(x => x.State));

            return await _collection.Find(query.FilterDefinition).Project<Provider>(query.ProjectionDefinition).FirstOrDefaultAsync();
        }
        public async Task<Provider> GetShortById(string id)
        {
            var query = DbQuery
                .Eq(x => x.Id, id)
                .Eq(x => x.State, true)
                .Projection(z => z
                    .Include(x => x.Id)
                    .Include(x => x.Name)
                    .Include(x => x.IsVisible)
                    .Include(x => x.IsLoginEnabled)
                    .Include(x => x.Slug)
                    .Include(x => x.UpdatedTime)
                    .Include(x => x.CreatedTime)
                    .Include(x => x.State));

            return await _collection.Find(query.FilterDefinition).Project<Provider>(query.ProjectionDefinition).FirstOrDefaultAsync();
        }

        #region Roles

        public async Task<List<ProviderRole>> GetRolesById(string id)
        {
            var query = DbQuery
                .Eq(x => x.Id, id)
                .Eq(x => x.State, true)
                .Projection(z => z.Include(x => x.Roles));

            return (await _collection.Find(query.FilterDefinition).Project<Provider>(query.ProjectionDefinition).FirstOrDefaultAsync())?.Roles ?? new List<ProviderRole>();
        }
        public async Task<List<ProviderRole>> GetRolesBySlug(string slug)
        {
            var query = _getSlugQuery(slug)
                .Projection(z => z.Include(x => x.Roles));

            return (await _collection.Find(query.FilterDefinition).Project<Provider>(query.ProjectionDefinition).FirstOrDefaultAsync())?.Roles ?? new List<ProviderRole>();
        }

        public async Task<bool> RoleNameExistsBySlug(string slug, string name)
        {
            var query = DbQuery.CustomSearch(z => z.And(
                z.Eq(x => x.Slug, slug),
                z.Eq(x => x.State, true),
                z.ElemMatch(x => x.Roles, x => x.Name == name)));

            return (await _collection.CountDocumentsAsync(query.FilterDefinition)) == 1;
        }

        public async Task<bool> InsertRole(string id, ProviderRole role)
        {
            var query = _getIdQuery(id)
                .Update(z => z.AddToSet(x => x.Roles, role))
                .Update(x => x.Set(z => z.UpdatedTime, DateTime.UtcNow));

            return (await _collection.UpdateOneAsync(query.FilterDefinition, query.UpdateDefinition)).MatchedCount == 1;
        }

        public async Task<bool> InsertRoleBySlug(string slug, ProviderRole role)
        {
            var query = _getSlugQuery(slug)
                .Update(z => z.AddToSet(x => x.Roles, role).Set(x => x.UpdatedTime, DateTime.UtcNow));

            return (await _collection.UpdateOneAsync(query.FilterDefinition, query.UpdateDefinition)).MatchedCount == 1;
        }


        public async Task<bool> RemoveRole(string id, string roleName)
        {
            var query = _getIdQuery(id)
                .Update(x => x.PullFilter(z => z.Roles, z => z.Name == roleName));

            return (await _collection.UpdateOneAsync(query.FilterDefinition, query.UpdateDefinition)).ModifiedCount == 1;
        }
        public async Task<bool> RemoveRoleBySlug(string slug, string roleName)
        {
            var query = _getSlugQuery(slug)
                .Update(x => x.PullFilter(z => z.Roles, z => z.Name == roleName));

            return (await _collection.UpdateOneAsync(query.FilterDefinition, query.UpdateDefinition)).ModifiedCount == 1;
        }

        #endregion Roles

        #region Fingerprints

        public async Task<bool> InsertFingerprint(string id, ProviderFingerprint fingerpeint)
        {
            var query = _getIdQuery(id)
                .Update(x => x.AddToSet(z => z.Fingerprints, fingerpeint))
                .Update(x => x.Set(z => z.UpdatedTime, DateTime.UtcNow));

            return (await _collection.UpdateOneAsync(query.FilterDefinition, query.UpdateDefinition)).ModifiedCount == 1;
        }

        public async Task<bool> RemoveFingerprint(string id, string fingerprintName)
        {
            var query = _getIdQuery(id)
                .Update(z => z.PullFilter(x => x.Fingerprints, x => x.Name == fingerprintName));

            return (await _collection.UpdateOneAsync(query.FilterDefinition, query.UpdateDefinition)).ModifiedCount == 1;
        }

        public async Task UpdateFingerprints(Provider entity)
        {
            var filter = DbQuery
                .Eq(x => x.Id, entity.Id)
                .Update(x => x.Set(z => z.Fingerprints, entity.Fingerprints).Set(z => z.UpdatedTime, DateTime.UtcNow));

            await _collection.UpdateOneAsync(filter.FilterDefinition, filter.UpdateDefinition);
        }

        public async Task<Provider> GetByFingerprint(string fingerprint)
        {
            var filter = DbQuery
                .CustomSearch(x => x.And(
                    x.Eq(z => z.State, true),
                    x.ElemMatch(z => z.Fingerprints, z => z.Fingerprint == fingerprint)));

            return await _collection.Find(filter.FilterDefinition).FirstOrDefaultAsync();
        }

        #endregion Fingerprints

        #region Workers

        public async Task<long> UserInWorkersCount(string userId)
        {
            var q = DbQuery.CustomSearch(x => x.And(
                x.Eq(z => z.State, true),
                x.ElemMatch(z => z.Workers, z => z.UserId == userId)));

            return await _collection.CountDocumentsAsync(q.FilterDefinition);
        }

        public async Task<bool> IsWorkerInRoleBySlug(string slug, string userId, ProviderWorkerRole role)
        {
            var filter = DbQuery.CustomSearch(z => z.And(
                z.Eq(x => x.Slug, slug.ToLower()),
                z.Eq(x => x.State, true),
                z.ElemMatch(x => x.Workers, x => x.UserId == userId && x.Roles.Contains(role))));

            return (await _collection.CountDocumentsAsync(filter.FilterDefinition)) == 1;
        }

        public async Task<bool> IsWorkerExistsBySlug(string slug, string userId)
        {
            var q = DbQuery.CustomSearch(x => x.And(
                x.Eq(z => z.Slug, slug.ToLower()),
                x.Eq(z => z.State, true),
                x.ElemMatch(z => z.Workers, z => z.UserId == userId)));

            return (await _collection.CountDocumentsAsync(q.FilterDefinition)) == 1;
        }

        public async Task<List<ProviderWorker>> GetWorkersBySlug(string slug)
        {
            var filter = _getSlugQuery(slug)
                .Projection(x => x.Include(z => z.Workers));

            return (await _collection.Find(filter.FilterDefinition).Project<Provider>(filter.ProjectionDefinition).FirstOrDefaultAsync())?.Workers ?? new List<ProviderWorker>();
        }

        public async Task<bool> InsertWorkersBySlug(string slug, ProviderWorker worker) => await InsertWorkersBySlug(slug, new List<ProviderWorker>() { worker });
        public async Task<bool> InsertWorkersBySlug(string slug, IEnumerable<ProviderWorker> workers)
        {
            var q = _getSlugQuery(slug)
                .Update(x => x.AddToSetEach(z => z.Workers, workers));

            return (await _collection.UpdateOneAsync(q.FilterDefinition, q.UpdateDefinition)).ModifiedCount == 1;
        }

        public async Task<bool> InsertWorkersById(string id, IEnumerable<ProviderWorker> workers)
        {
            var q = _getIdQuery(id)
                .Update(x => x.AddToSetEach(z => z.Workers, workers));

            return (await _collection.UpdateOneAsync(q.FilterDefinition, q.UpdateDefinition)).ModifiedCount == 1;
        }

        public async Task<bool> RemoveWorkerBySlug(string slug, string userId)
        {
            var q = _getSlugQuery(slug)
                .Update(x => x.PullFilter(z => z.Workers, z => z.UserId == userId));

            return (await _collection.UpdateOneAsync(q.FilterDefinition, q.UpdateDefinition)).ModifiedCount == 1;
        }

        public async Task<bool> RemoveWorker(string id, string userId)
        {
            var q = _getIdQuery(id)
                .Update(x => x.PullFilter(z => z.Workers, z => z.UserId == userId));

            return (await _collection.UpdateOneAsync(q.FilterDefinition, q.UpdateDefinition)).ModifiedCount == 1;
        }

        public async Task<bool> UpdateWorkerBySlug(string slug, string userId, IEnumerable<ProviderWorkerRole> roles)
        {
            var q = DbQuery.CustomSearch(z => z.And(
                z.Eq(x => x.Slug, slug),
                z.Eq(x => x.State, true),
                z.ElemMatch(x => x.Workers, x => x.UserId == userId)))
                .Update(x => x.Set(z => z.Workers[0].Roles, roles));

            return (await _collection.UpdateOneAsync(q.FilterDefinition, q.UpdateDefinition)).ModifiedCount == 1;
        }

        public async Task<bool> UpdateWorker(string id, string userId, IEnumerable<ProviderWorkerRole> roles)
        {
            var q = DbQuery.CustomSearch(z => z.And(
                z.Eq(x => x.Id, id),
                z.Eq(x => x.State, true),
                z.ElemMatch(x => x.Workers, x => x.UserId == userId)))
                .Update(x => x.Set(z => z.Workers[0].Roles, roles));

            return (await _collection.UpdateOneAsync(q.FilterDefinition, q.UpdateDefinition)).ModifiedCount == 1;
        }

        #endregion Workers

        public async Task<bool> ExistsWithOwner(string id, string userId)
        {
            return (await _collection.CountDocumentsAsync(x => x.Id == id && x.Owner != null && x.State && x.Owner.Id == userId)) == 1;
        }

        public async Task<bool> ExistsWithOwnerSlug(string slug, string userId)
        {
            var queury = DbQuery
                .Eq(x => x.Slug, slug)
                .Eq(x => x.State, true)
                .Eq(x => x.Owner.Id, userId);

            return (await _collection.CountDocumentsAsync(DbQuery.FilterDefinition)) == 1;
        }

        protected DbQuery<Provider> _getIdQuery(string id) => DbQuery.Eq(x => x.Id, id).Eq(x => x.State, true);
        protected DbQuery<Provider> _getSlugQuery(string slug) => DbQuery.Eq(x => x.Slug, slug).Eq(x => x.State, true);
    }
}
