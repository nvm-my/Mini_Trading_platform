using TradingPlatform.Models;
using TradingPlatform.Repositories;

namespace TradingPlatform.Services
{
    public class MatchingEngineService
    {
        private readonly OrderRepository _orderRepo;
        private readonly TradeRepository _tradeRepo;
        private readonly BillingService _billingService;
        private readonly FixMessageService _fixMessageService;

        public MatchingEngineService(
            OrderRepository orderRepo,
            TradeRepository tradeRepo,
            BillingService billingService,
            FixMessageService fixMessageService)
        {
            _orderRepo = orderRepo;
            _tradeRepo = tradeRepo;
            _billingService = billingService;
            _fixMessageService = fixMessageService;
        }

        public async Task MatchAsync(Order incomingOrder)
        {
            // Fetch opposite orders
            var oppositeSide = incomingOrder.Side == "BUY" ? "SELL" : "BUY";

            var oppositeOrders = await _orderRepo
                .GetOpenOrdersByInstrumentAndSide(incomingOrder.InstrumentId, oppositeSide);

            // Sort by price-time priority
            if (incomingOrder.Side == "BUY")
                oppositeOrders = oppositeOrders.OrderBy(o => o.Price).ToList();
            else
                oppositeOrders = oppositeOrders.OrderByDescending(o => o.Price).ToList();

            foreach (var order in oppositeOrders)
            {
                if (incomingOrder.RemainingQuantity <= 0)
                    break;

                // Price check
                if (incomingOrder.OrderType == "LIMIT")
                {
                    if (incomingOrder.Side == "BUY" && incomingOrder.Price < order.Price)
                        continue;

                    if (incomingOrder.Side == "SELL" && incomingOrder.Price > order.Price)
                        continue;
                }

                int tradedQty = Math.Min(incomingOrder.RemainingQuantity, order.RemainingQuantity);

                var trade = new Trade
                {
                    BuyOrderId = incomingOrder.Side == "BUY" ? incomingOrder.Id : order.Id,
                    SellOrderId = incomingOrder.Side == "SELL" ? incomingOrder.Id : order.Id,
                    InstrumentId = incomingOrder.InstrumentId,
                    Price = order.Price ?? 0,
                    Quantity = tradedQty
                };

                await _tradeRepo.CreateAsync(trade);

                // Update quantities
                incomingOrder.RemainingQuantity -= tradedQty;
                order.RemainingQuantity -= tradedQty;

                // Record FIX ExecutionReports for both sides of the trade
                var buyOrder = incomingOrder.Side == "BUY" ? incomingOrder : order;
                var sellOrder = incomingOrder.Side == "SELL" ? incomingOrder : order;
                await _fixMessageService.RecordExecutionReportsAsync(buyOrder, sellOrder, trade);

                await _billingService.ProcessTrade(trade);

                if (order.RemainingQuantity == 0)
                    order.Status = "FILLED";

                await _orderRepo.UpdateAsync(order);
            }

            if (incomingOrder.RemainingQuantity == 0)
                incomingOrder.Status = "FILLED";
            else
                incomingOrder.Status = "PARTIAL";

            await _orderRepo.UpdateAsync(incomingOrder);
        }
    }
}