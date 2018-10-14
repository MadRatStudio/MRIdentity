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
    public class ProviderCategoryRepository : BaseRepository<ProviderCategory>, IRepository<ProviderCategory>
    {
        public ProviderCategoryRepository(IMongoDatabase mongoDatabase) : base(mongoDatabase)
        {
        }

        public async Task<ICollection<ProviderCategory>> GetAll(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any()) return new List<ProviderCategory>();

            var query = DbQuery
                .Where(x => x.State)
                .Contains(x => x.Id, ids)
                .Descending(x => x.CreatedTime);

            return await Get(query);
        }

        public async Task<ICollection<ProviderCategory>> Search(int skip, int limit, string q)
        {
            DbQuery<ProviderCategory> query = null;

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = DbQuery.CustomSearch(x =>
                x.And(
                    x.Where(z => z.State),
                    x.ElemMatch(z => z.Translations, z => z.Name.Contains(q))));
            }
            else
            {
                query = DbQuery.Where(x => x.State);
            }

            query.Skip = skip;
            query.Limit = limit;

            return await Get(query);
        }
    }
}
