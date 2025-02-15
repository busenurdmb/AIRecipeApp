using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace AIRecipeApp.Api.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }
    }
}
