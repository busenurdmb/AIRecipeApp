using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace AIRecipeApp.Api.Entities
{
    public class Recipe
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("ingredients")]
        public List<string> Ingredients { get; set; }

        [BsonElement("instructions")]
        public string Instructions { get; set; }

        [BsonElement("userId")]
        public string? UserId { get; set; }
    }
}
