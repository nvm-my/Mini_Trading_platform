using TradingPlatform.Models;
using TradingPlatform.Repositories.Interfaces;

namespace TradingPlatform.Services
{
    /// <summary>
    /// Settles executed trades by adjusting buyer and seller wallet balances.
    /// </summary>
    public class BillingService
    {
        private readonly IUserRepository _userRepo;

        public BillingService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        /// <summary>
        /// Debits the buyer's wallet and credits the seller's wallet for the given trade.
        /// </summary>
        public async Task ProcessTrade(Trade trade)
        {
            var buyer = await _userRepo.GetByIdAsync(trade.BuyOrderId);
            var seller = await _userRepo.GetByIdAsync(trade.SellOrderId);

            if (buyer == null || seller == null || trade.Quantity == 0)
                return;

            decimal total = trade.Price * trade.Quantity;

            buyer.WalletBalance -= total;
            seller.WalletBalance += total;

            await _userRepo.UpdateAsync(buyer);
            await _userRepo.UpdateAsync(seller);
        }
    }
}
