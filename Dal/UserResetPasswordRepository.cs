using Infrastructure.Entities;
using MongoDB.Driver;
using MRDb.Infrastructure.Interface;
using MRDb.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dal
{
    public class UserResetPasswordRepository : BaseRepository<UserResetPassword>, IRepository<UserResetPassword>
    {
        public UserResetPasswordRepository(IMongoDatabase mongoDatabase) : base(mongoDatabase) { }

        public async Task<UserResetPassword> GetByUser(string userId, string code)
        {
            var query =
                DbQuery
                    .Eq(x => x.UserId, userId)
                    .Eq(x => x.Code, code)
                    .Ascending(x => x.CreatedTime);

            return await GetFirst(query);
        }

        public async Task ResetAllCodes(string userId)
        {
            var query = DbQuery.Where(x => x.UserId == userId && x.State);
            query.Update(builder => builder.Set(x => x.State, false));

            await _collection.UpdateManyAsync(query.FilterDefinition, query.UpdateDefinition);
        }
    }
}
