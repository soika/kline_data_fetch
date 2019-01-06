namespace Core.Data.Repositories
{
    using MongoDbGenericRepository;
    using MongoDB.Driver;

    public class AppMongoRepository : BaseMongoRepository
    {
        public AppMongoRepository(string connectionString,
                                  string databaseName) : base(connectionString, databaseName)
        {
        }

        public AppMongoRepository(IMongoDbContext mongoDbContext) : base(mongoDbContext)
        {
        }

        public AppMongoRepository(IMongoDatabase mongoDatabase) : base(mongoDatabase)
        {
        }
    }
}