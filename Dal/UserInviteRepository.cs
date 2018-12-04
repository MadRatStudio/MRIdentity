using Infrastructure.Entities;
using MongoDB.Driver;
using MRDb.Infrastructure.Interface;
using MRDb.Repository;
using System.Threading.Tasks;

namespace Dal
{
    public class UserInviteRepository : BaseRepository<UserInvite>, IRepository<UserInvite>
    {
        public UserInviteRepository(IMongoDatabase mongoDatabase) : base(mongoDatabase) { }

        public async Task<UserInvite> GetByCode(string code)
        {
            var q = DbQuery
                .Eq(x => x.State, true)
                .Eq(x => x.Code, code);

            return await _collection.Find(q.FilterDefinition).FirstOrDefaultAsync();
        }
    }
}
