using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ImagineHackStorage.Models
{
    [BsonIgnoreExtraElements]
    public class MongoEntity<TKey>
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public TKey Id { get; set; }
    }
}