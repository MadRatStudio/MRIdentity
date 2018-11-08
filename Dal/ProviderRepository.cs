using Infrastructure.Entities;
using MongoDB.Driver;
using MRDb.Infrastructure.Interface;
using MRDb.Repository;
using System;
using System.Threading.Tasks;

namespace Dal
{
    public class ProviderRepository : BaseRepository<Provider>, IRepository<Provider>
    {
        public ProviderRepository(IMongoDatabase mongoDatabase) : base(mongoDatabase) { }

        public async Task UpdateFingerprints(Provider entity)
        {
            var filter = DbQuery
                .Eq(x => x.Id, entity.Id)
                .Update(x => x.Set(z => z.Fingerprints, entity.Fingerprints))
                .Update(x => x.Set(z => z.UpdatedTime, DateTime.UtcNow));

            await _collection.UpdateOneAsync(filter.FilterDefinition, filter.UpdateDefinition);
        }

        public async Task<bool> ExistsWithOwner(string id, string userId)
        {
            return (await _collection.CountDocumentsAsync(x => x.Id == id && x.Owner != null && x.State && x.Owner.Id == userId)) == 1;
        }
    }
}
