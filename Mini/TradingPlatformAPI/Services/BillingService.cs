using TradingPlatform.Models;
using TradingPlatform.Repositories;

namespace TradingPlatform.Services
{
    public class BillingService
    {
        private readonly UserRepository _userRepo;

        public BillingService(UserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task ProcessTrade(Trade trade)
        {
            var buyer = await _userRepo.GetByIdAsync(trade.BuyOrderId);
            var seller = await _userRepo.GetByIdAsync(trade.SellOrderId);

            decimal total = trade.Price * trade.Quantity;

            // Deduct buyer balance
            buyer.WalletBalance -= total;

            // Add seller balance
            seller.WalletBalance += total;

            await _userRepo.UpdateAsync(buyer);
            await _userRepo.UpdateAsync(seller);
        }
    }
}