using TradingPlatform.Models;
using TradingPlatform.Repositories.Interfaces;

namespace TradingPlatform.Services
{
    /// <summary>
    /// Implements a price-time priority order matching engine.
    /// For each incoming order it attempts to match against resting opposite-side orders,
    /// creating <see cref="Trade"/> records, generating FIX ExecutionReports, and
    /// triggering billing settlement for every matched quantity.
    /// </summary>
    public class MatchingEngineService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ITradeRepository _tradeRepo;
        private readonly BillingService _billingService;
        private readonly FixMessageService _fixMessageService;

        public MatchingEngineService(
            IOrderRepository orderRepo,
            ITradeRepository tradeRepo,
            BillingService billingService,
            FixMessageService fixMessageService)
        {
            _orderRepo = orderRepo;
            _tradeRepo = tradeRepo;
            _billingService = billingService;
            _fixMessageService = fixMessageService;
        }

        /// <summary>
        /// Attempts to match <paramref name="incomingOrder"/> against resting orders on the
        /// opposite side, executing trades and updating order statuses accordingly.
        /// </summary>
        public async Task MatchAsync(Order incomingOrder)
        {
            var oppositeSide = incomingOrder.Side == "BUY" ? "SELL" : "BUY";

            var oppositeOrders = await _orderRepo
                .GetOpenOrdersByInstrumentAndSide(incomingOrder.InstrumentId, oppositeSide);

            // Sort by price-time priority
            if (incomingOrder.Side == "BUY")
                oppositeOrders = oppositeOrders.OrderBy(o => o.Price).ToList();
            else
                oppositeOrders = oppositeOrders.OrderByDescending(o => o.Price).ToList();

            foreach (var restingOrder in oppositeOrders)
            {
                if (incomingOrder.RemainingQuantity <= 0)
                    break;

                // Price check for LIMIT orders
                if (incomingOrder.OrderType == "LIMIT")
                {
                    if (incomingOrder.Side == "BUY" && incomingOrder.Price < restingOrder.Price)
                        continue;

                    if (incomingOrder.Side == "SELL" && incomingOrder.Price > restingOrder.Price)
                        continue;
                }

                int tradedQty = Math.Min(incomingOrder.RemainingQuantity, restingOrder.RemainingQuantity);

                var trade = new Trade
                {
                    BuyOrderId = incomingOrder.Side == "BUY" ? incomingOrder.Id : restingOrder.Id,
                    SellOrderId = incomingOrder.Side == "SELL" ? incomingOrder.Id : restingOrder.Id,
                    InstrumentId = incomingOrder.InstrumentId,
                    Price = restingOrder.Price ?? 0,
                    Quantity = tradedQty
                };

                await _tradeRepo.CreateAsync(trade);

                incomingOrder.RemainingQuantity -= tradedQty;
                restingOrder.RemainingQuantity -= tradedQty;

                var buyOrder = incomingOrder.Side == "BUY" ? incomingOrder : restingOrder;
                var sellOrder = incomingOrder.Side == "SELL" ? incomingOrder : restingOrder;
                await _fixMessageService.RecordExecutionReportsAsync(buyOrder, sellOrder, trade);

                await _billingService.ProcessTrade(trade);

                if (restingOrder.RemainingQuantity == 0)
                    restingOrder.Status = "FILLED";

                await _orderRepo.UpdateAsync(restingOrder);
            }

            incomingOrder.Status = incomingOrder.RemainingQuantity == 0 ? "FILLED" : "PARTIAL";

            await _orderRepo.UpdateAsync(incomingOrder);
        }
    }
}
