using Infrastructure.Entities;
using MongoDB.Driver;
using MRDb.Infrastructure.Interface;
using MRDb.Repository;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dal
{
    public class LanguageRepository : BaseRepository<Language>, IRepository<Language>
    {
        public LanguageRepository(IMongoDatabase mongoDatabase) : base(mongoDatabase)
        {
        }

        public async Task<IEnumerable<Language>> Search(int skip, int limit, string q)
        {
            var filter = DbQuery;

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.ToLower();

                filter = DbQuery.CustomSearch(x
                    => x.Or(
                        x.Regex(z => z.Code, new MongoDB.Bson.BsonRegularExpression($"{q}", "i")),
                        x.Regex(z => z.Name, new MongoDB.Bson.BsonRegularExpression($"{q}", "i")),
                        x.Regex(z => z.NativeName, new MongoDB.Bson.BsonRegularExpression($"{q}", "i"))));
            }


            filter.Limit = limit;
            filter.Skip = skip;
            filter.Descending(x => x.CreatedTime);

            return await Get(filter);
        }
    }
}
