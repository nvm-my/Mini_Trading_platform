using TradingPlatform.Models;
using TradingPlatform.Repositories;
using TradingPlatform.Services;
using Moq;
using MongoDB.Driver;

namespace TradingPlatformAPI.Tests;

public class BillingServiceTests
{
    // ------------------------------------------------------------------ //
    // Helpers
    // ------------------------------------------------------------------ //

    private static (BillingService svc, Mock<IMongoCollection<User>> mockUsers)
        BuildService()
    {
        var mockUsers = MongoTestHelper.CreateCollection<User>();
        var mockDb = MongoTestHelper.RegisterCollection<User>(null, "Users", mockUsers);
        var userRepo = new UserRepository(mockDb.Object);
        return (new BillingService(userRepo), mockUsers);
    }

    private static User MakeUser(string id, decimal balance) =>
        new() { Id = id, Name = "Test", Email = $"{id}@test.com",
                PasswordHash = "hash", Role = "Client", WalletBalance = balance };

    // ------------------------------------------------------------------ //
    // ProcessTrade
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task ProcessTrade_DeductsBuyerAndCreditsSeller()
    {
        var (svc, mockUsers) = BuildService();

        var buyer = MakeUser("buyer-id", 1_000m);
        var seller = MakeUser("seller-id", 200m);

        var trade = new Trade
        {
            Id = "t-001",
            BuyOrderId = buyer.Id,
            SellOrderId = seller.Id,
            InstrumentId = "ACME",
            Price = 50m,
            Quantity = 4,  // total = 200
        };

        // Queue: first Find → buyer, second Find → seller
        MongoTestHelper.SetupFind(mockUsers, new[] { buyer }, new[] { seller });

        await svc.ProcessTrade(trade);

        Assert.Equal(800m, buyer.WalletBalance);   // 1000 - 200
        Assert.Equal(400m, seller.WalletBalance);  // 200 + 200
    }

    [Fact]
    public async Task ProcessTrade_UpdatesBothUsersInDb()
    {
        var (svc, mockUsers) = BuildService();

        var buyer = MakeUser("buyer-id", 500m);
        var seller = MakeUser("seller-id", 100m);

        var trade = new Trade
        {
            Id = "t-002",
            BuyOrderId = buyer.Id,
            SellOrderId = seller.Id,
            InstrumentId = "BRACBANK",
            Price = 25m,
            Quantity = 2,  // total = 50
        };

        MongoTestHelper.SetupFind(mockUsers, new[] { buyer }, new[] { seller });

        await svc.ProcessTrade(trade);

        // ReplaceOneAsync must have been called for both buyer and seller
        mockUsers.Verify(
            c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<User>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessTrade_ZeroQuantity_BalancesUnchanged()
    {
        var (svc, mockUsers) = BuildService();

        var buyer = MakeUser("buyer-id", 300m);
        var seller = MakeUser("seller-id", 100m);

        var trade = new Trade
        {
            Id = "t-003",
            BuyOrderId = buyer.Id,
            SellOrderId = seller.Id,
            InstrumentId = "ACME",
            Price = 10m,
            Quantity = 0,
        };

        MongoTestHelper.SetupFind(mockUsers, new[] { buyer }, new[] { seller });

        await svc.ProcessTrade(trade);

        Assert.Equal(300m, buyer.WalletBalance);
        Assert.Equal(100m, seller.WalletBalance);
    }
}
