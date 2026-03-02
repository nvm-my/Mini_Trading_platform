using TradingPlatform.DTOs;
using TradingPlatform.Models;
using TradingPlatform.Repositories;

namespace TradingPlatform.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepo;
        private readonly MatchingEngineService _matchingEngine;

        public OrderService(
            OrderRepository orderRepo,
            MatchingEngineService matchingEngine)
        {
            _orderRepo = orderRepo;
            _matchingEngine = matchingEngine;
        }

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

            // Immediately try matching
            await _matchingEngine.MatchAsync(order);

            return order;
        }

        public async Task CancelOrder(string orderId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
                throw new Exception("Order not found");

            order.Status = "CANCELLED";
            await _orderRepo.UpdateAsync(order);
        }
    }
}