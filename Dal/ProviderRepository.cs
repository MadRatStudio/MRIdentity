using Infrastructure.Entities;
using MongoDB.Driver;
using MRDb.Infrastructure.Interface;
using MRDb.Repository;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dal
{
    public class ProviderRepository : BaseRepository<Provider>, IRepository<Provider>
    {
        public ProviderRepository(IMongoDatabase mongoDatabase) : base(mongoDatabase)
        {
        }
    }
}
