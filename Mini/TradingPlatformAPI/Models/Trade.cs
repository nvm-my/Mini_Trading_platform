using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TradingPlatform.Models
{
    public class Trade
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string BuyOrderId { get; set; }

        public string SellOrderId { get; set; }

        public string InstrumentId { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    }
}