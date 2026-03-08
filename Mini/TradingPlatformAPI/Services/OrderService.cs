using TradingPlatform.DTOs;
using TradingPlatform.Models;
using TradingPlatform.Repositories.Interfaces;

namespace TradingPlatform.Services
{
    /// <summary>
    /// Handles order placement and cancellation, delegating matching to
    /// <see cref="MatchingEngineService"/> after an order is persisted.
    /// </summary>
    public class OrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly MatchingEngineService _matchingEngine;

        public OrderService(
            IOrderRepository orderRepo,
            MatchingEngineService matchingEngine)
        {
            _orderRepo = orderRepo;
            _matchingEngine = matchingEngine;
        }

        /// <summary>
        /// Persists the order then triggers the matching engine synchronously.
        /// </summary>
        public async Task<Order> PlaceOrder(string userId, OrderDTO dto)
        {
            var order = new Order
            {
                UserId = userId,
                InstrumentId = dto.InstrumentId,
                Side = dto.Side,
                OrderType = dto.OrderType,
                Price = dto.Price,
                Quantity = dto.Quantity,
                RemainingQuantity = dto.Quantity
            };

            await _orderRepo.CreateAsync(order);
            await _matchingEngine.MatchAsync(order);

            return order;
        }

        /// <summary>
        /// Cancels an open order by setting its status to CANCELLED.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown when the order does not exist.</exception>
        public async Task CancelOrder(string orderId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException("Order not found");

            order.Status = "CANCELLED";
            await _orderRepo.UpdateAsync(order);
        }
    }
}
