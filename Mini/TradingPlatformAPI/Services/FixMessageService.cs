using System.Globalization;
using System.Text;
using TradingPlatform.Models;
using TradingPlatform.Repositories;

namespace TradingPlatform.Services
{
    /// <summary>
    /// Builds FIX 4.2 ExecutionReport messages and persists them in MongoDB.
    /// Called by <see cref="MatchingEngineService"/> after every trade execution.
    /// </summary>
    public class FixMessageService
    {
        private readonly FixMessageRepository _fixMessageRepo;

        public FixMessageService(FixMessageRepository fixMessageRepo)
        {
            _fixMessageRepo = fixMessageRepo;
        }

        /// <summary>
        /// Creates one FIX ExecutionReport for the buy order and one for the sell order,
        /// then persists both in the FixMessages collection.
        /// </summary>
        public async Task RecordExecutionReportsAsync(Order buyOrder, Order sellOrder, Trade trade)
        {
            var buyReport = CreateExecutionReport(buyOrder, trade, "1");   // FIX side 1 = Buy
            var sellReport = CreateExecutionReport(sellOrder, trade, "2"); // FIX side 2 = Sell

            await _fixMessageRepo.CreateAsync(buyReport);
            await _fixMessageRepo.CreateAsync(sellReport);
        }

        /// <summary>
        /// Builds a <see cref="FixMessage"/> (ExecutionReport) for one side of a trade.
        /// </summary>
        /// <param name="order">The order on this side (quantities must already be updated).</param>
        /// <param name="trade">The executed trade.</param>
        /// <param name="fixSide">"1" for Buy, "2" for Sell.</param>
        public FixMessage CreateExecutionReport(Order order, Trade trade, string fixSide)
        {
            int cumQty = order.Quantity - order.RemainingQuantity;

            // ExecType: F=Filled, 1=PartialFill
            string execType = order.RemainingQuantity == 0 ? "F" : "1";

            // OrdStatus: 2=Filled, 1=PartiallyFilled
            string ordStatus = order.RemainingQuantity == 0 ? "2" : "1";

            var msg = new FixMessage
            {
                MsgType = "8",
                ClOrdId = order.Id,
                OrderId = order.Id,
                ExecId = Guid.NewGuid().ToString("N"),
                ExecType = execType,
                OrdStatus = ordStatus,
                Symbol = order.InstrumentId,
                Side = fixSide,
                Price = order.Price ?? trade.Price,
                OrderQty = order.Quantity,
                LastPx = trade.Price,
                LastQty = trade.Quantity,
                LeavesQty = order.RemainingQuantity,
                CumQty = cumQty,
                TradeId = trade.Id,
                TransactTime = DateTime.UtcNow,
            };

            msg.RawMessage = BuildRawFixMessage(msg);
            return msg;
        }

        /// <summary>
        /// Serialises a <see cref="FixMessage"/> into a FIX 4.2 wire-format string.
        /// Fields are separated by SOH (ASCII 0x01). Body length and checksum are computed
        /// according to the FIX specification.
        /// </summary>
        private static string BuildRawFixMessage(FixMessage m)
        {
            const char Soh = '\u0001';

            // Body fields in FIX tag order (BeginString / BodyLength / Checksum are added separately)
            var bodyParts = new (int Tag, string Value)[]
            {
                (35,  m.MsgType),
                (49,  "SERVER"),
                (56,  "CLIENT"),
                (11,  m.ClOrdId  ?? string.Empty),
                (37,  m.OrderId  ?? string.Empty),
                (17,  m.ExecId   ?? string.Empty),
                (150, m.ExecType),
                (39,  m.OrdStatus),
                (55,  m.Symbol   ?? string.Empty),
                (54,  m.Side),
                (44,  m.Price.ToString("F2", CultureInfo.InvariantCulture)),
                (38,  m.OrderQty.ToString(CultureInfo.InvariantCulture)),
                (31,  m.LastPx.ToString("F2", CultureInfo.InvariantCulture)),
                (32,  m.LastQty.ToString(CultureInfo.InvariantCulture)),
                (151, m.LeavesQty.ToString(CultureInfo.InvariantCulture)),
                (14,  m.CumQty.ToString(CultureInfo.InvariantCulture)),
                (60,  m.TransactTime.ToUniversalTime().ToString("yyyyMMdd-HH:mm:ss.fff", CultureInfo.InvariantCulture)),
            };

            var body = string.Concat(bodyParts.Select(p => $"{p.Tag}={p.Value}{Soh}"));
            int bodyLen = Encoding.ASCII.GetByteCount(body);

            var header = $"8=FIX.4.2{Soh}9={bodyLen}{Soh}";
            var msgWithoutChecksum = header + body;

            int checksum = Encoding.ASCII
                .GetBytes(msgWithoutChecksum)
                .Aggregate(0, (acc, b) => acc + b) % 256;

            return $"{msgWithoutChecksum}10={checksum:D3}{Soh}";
        }
    }
}
