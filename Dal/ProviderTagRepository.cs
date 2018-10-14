using Infrastructure.Entities;
using MongoDB.Driver;
using MRDb.Infrastructure.Interface;
using MRDb.Repository;
using MRDb.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dal
{
    public class ProviderTagRepository : BaseRepository<ProviderTag>, IRepository<ProviderTag>
    {
        public ProviderTagRepository(IMongoDatabase mongoDatabase) : base(mongoDatabase)
        {
        }

        public async Task<ICollection<ProviderTag>> GetAll(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any()) return new List<ProviderTag>();

            var query = DbQuery
                .Where(x => x.State)
                .Contains(x => x.Id, ids)
                .Descending(x => x.CreatedTime);

            return await Get(query);
        }

        public async Task<ICollection<ProviderTag>> Search(int skip, int limit, string q)
        {
            DbQuery<ProviderTag> query = null;
            if (!string.IsNullOrWhiteSpace(q))
            {

                query = DbQuery
                    .CustomSearch(
                    x => x.And(
                            x.Where(z => z.State),
                            x.Or(
                                x.Regex(z => z.Key, new MongoDB.Bson.BsonRegularExpression(q, "i")),
                                x.ElemMatch(z => z.Translations, z => z.Name.ToLower().Contains(q.ToLower()))
                                )))
                    .Descending(x => x.CreatedTime);
            }
            else
            {
                query = DbQuery.Where(x => x.State).Descending(x => x.CreatedTime);
            }

            query.Skip = skip;
            query.Limit = limit;

            return await Get(query);
        }
    }
}
