using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TradingPlatform.Models
{
    /// <summary>
    /// Represents a FIX 4.2 ExecutionReport (MsgType=8) stored in MongoDB.
    /// One record is created for each side (BUY / SELL) every time a trade is executed
    /// by the matching engine.
    /// </summary>
    public class FixMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // Tag 35 – always "8" (ExecutionReport)
        public string MsgType { get; set; } = "8";

        // Tag 11 – client-assigned order reference
        public string ClOrdId { get; set; }

        // Tag 37 – server-assigned order ID
        public string OrderId { get; set; }

        // Tag 17 – unique execution identifier
        public string ExecId { get; set; }

        // Tag 150 – "F"=Fill, "1"=PartialFill
        public string ExecType { get; set; }

        // Tag 39 – "2"=Filled, "1"=PartiallyFilled
        public string OrdStatus { get; set; }

        // Tag 55 – instrument identifier (InstrumentId used as symbol proxy)
        public string Symbol { get; set; }

        // Tag 54 – "1"=Buy, "2"=Sell
        public string Side { get; set; }

        // Tag 44 – order limit price (or last trade price for MARKET orders)
        public decimal Price { get; set; }

        // Tag 38 – original order quantity
        public int OrderQty { get; set; }

        // Tag 31 – price of this execution
        public decimal LastPx { get; set; }

        // Tag 32 – quantity of this execution
        public int LastQty { get; set; }

        // Tag 151 – quantity still open after this execution
        public int LeavesQty { get; set; }

        // Tag 14 – total quantity filled so far
        public int CumQty { get; set; }

        // Reference to the Trade document that triggered this report
        public string TradeId { get; set; }

        // Tag 60 – time of execution (UTC)
        public DateTime TransactTime { get; set; } = DateTime.UtcNow;

        // Full FIX-formatted message string (SOH-delimited, with checksum)
        public string RawMessage { get; set; }
    }
}
