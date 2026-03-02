using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TradingPlatform.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }

        public string InstrumentId { get; set; }

        // BUY or SELL
        public string Side { get; set; }

        // MARKET or LIMIT
        public string OrderType { get; set; }

        public decimal? Price { get; set; }

        public int Quantity { get; set; }

        public int RemainingQuantity { get; set; }

        // OPEN / PARTIAL / FILLED / CANCELLED
        public string Status { get; set; } = "OPEN";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}