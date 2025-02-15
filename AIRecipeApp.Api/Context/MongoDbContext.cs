using AIRecipeApp.Api.Entities;
using MongoDB.Driver;

namespace AIRecipeApp.Api.Context
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            _database = client.GetDatabase(config["MongoDB:DatabaseName"]);
        }

        public IMongoCollection<Recipe> Recipes => _database.GetCollection<Recipe>("Recipes");
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

    }
}
