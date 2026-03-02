using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TradingPlatform.Models
{
    public class Instrument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Symbol { get; set; }

        public string CompanyName { get; set; }

        public decimal CurrentPrice { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}