using TradingPlatform.Models;
using TradingPlatform.Repositories;
using TradingPlatform.Services;
using Moq;
using MongoDB.Driver;

namespace TradingPlatformAPI.Tests;

public class MatchingEngineServiceTests
{
    // ------------------------------------------------------------------ //
    // Helpers
    // ------------------------------------------------------------------ //

    private static (MatchingEngineService svc,
                    Mock<IMongoCollection<Order>> mockOrders,
                    Mock<IMongoCollection<Trade>> mockTrades,
                    Mock<IMongoCollection<User>>  mockUsers,
                    Mock<IMongoCollection<FixMessage>> mockFix)
        BuildService()
    {
        var mockOrders = MongoTestHelper.CreateCollection<Order>();
        var mockTrades = MongoTestHelper.CreateCollection<Trade>();
        var mockUsers  = MongoTestHelper.CreateCollection<User>();
        var mockFix    = MongoTestHelper.CreateCollection<FixMessage>();

        var mockDb = new Moq.Mock<IMongoDatabase>();
        MongoTestHelper.RegisterCollection(mockDb, "Orders",      mockOrders);
        MongoTestHelper.RegisterCollection(mockDb, "Trades",      mockTrades);
        MongoTestHelper.RegisterCollection(mockDb, "Users",       mockUsers);
        MongoTestHelper.RegisterCollection(mockDb, "FixMessages", mockFix);

        var orderRepo  = new OrderRepository(mockDb.Object);
        var tradeRepo  = new TradeRepository(mockDb.Object);
        var userRepo   = new UserRepository(mockDb.Object);
        var fixRepo    = new FixMessageRepository(mockDb.Object);
        var fixSvc     = new FixMessageService(fixRepo);
        var billingSvc = new BillingService(userRepo);

        return (new MatchingEngineService(orderRepo, tradeRepo, billingSvc, fixSvc),
                mockOrders, mockTrades, mockUsers, mockFix);
    }

    private static Order MakeOpenSellOrder(
        string id = "sell-001",
        string instrumentId = "ACME",
        decimal price = 100m,
        int qty = 10) =>
        new()
        {
            Id = id,
            UserId = "seller",
            InstrumentId = instrumentId,
            Side = "SELL",
            OrderType = "LIMIT",
            Price = price,
            Quantity = qty,
            RemainingQuantity = qty,
            Status = "OPEN",
        };

    private static Order MakeIncomingBuyOrder(
        string id = "buy-001",
        string instrumentId = "ACME",
        string orderType = "LIMIT",
        decimal? price = 100m,
        int qty = 5) =>
        new()
        {
            Id = id,
            UserId = "buyer",
            InstrumentId = instrumentId,
            Side = "BUY",
            OrderType = orderType,
            Price = price,
            Quantity = qty,
            RemainingQuantity = qty,
            Status = "OPEN",
        };

    // ------------------------------------------------------------------ //
    // Full-fill scenario
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task MatchAsync_BuyMatchesSell_IncomingOrderIsFilled()
    {
        var (svc, mockOrders, _, mockUsers, _) = BuildService();

        var sellOrder = MakeOpenSellOrder(price: 100m, qty: 5);
        MongoTestHelper.SetupFind(mockOrders, new[] { sellOrder });
        MongoTestHelper.SetupFind(
            mockUsers,
            new[] { new User { Id = "buyer",  WalletBalance = 2_000m } },
            new[] { new User { Id = "seller", WalletBalance = 0m } });

        var buyOrder = MakeIncomingBuyOrder(price: 100m, qty: 5);
        await svc.MatchAsync(buyOrder);

        Assert.Equal("FILLED", buyOrder.Status);
        Assert.Equal(0, buyOrder.RemainingQuantity);
    }

    [Fact]
    public async Task MatchAsync_BuyMatchesSell_ExecutesTradeInDb()
    {
        var (svc, mockOrders, mockTrades, mockUsers, _) = BuildService();

        var sellOrder = MakeOpenSellOrder(price: 100m, qty: 5);
        MongoTestHelper.SetupFind(mockOrders, new[] { sellOrder });
        MongoTestHelper.SetupFind(
            mockUsers,
            new[] { new User { Id = "buyer",  WalletBalance = 1_000m } },
            new[] { new User { Id = "seller", WalletBalance = 0m } });

        var buyOrder = MakeIncomingBuyOrder(price: 100m, qty: 5);
        await svc.MatchAsync(buyOrder);

        mockTrades.Verify(
            c => c.InsertOneAsync(
                It.IsAny<Trade>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MatchAsync_BuyMatchesSell_RecordsFIXMessagesForBothSides()
    {
        var (svc, mockOrders, _, mockUsers, mockFix) = BuildService();

        var sellOrder = MakeOpenSellOrder(price: 100m, qty: 5);
        MongoTestHelper.SetupFind(mockOrders, new[] { sellOrder });
        MongoTestHelper.SetupFind(
            mockUsers,
            new[] { new User { Id = "buyer",  WalletBalance = 1_000m } },
            new[] { new User { Id = "seller", WalletBalance = 0m } });

        var buyOrder = MakeIncomingBuyOrder(price: 100m, qty: 5);
        await svc.MatchAsync(buyOrder);

        // One FIX ExecutionReport per side = 2 inserts
        mockFix.Verify(
            c => c.InsertOneAsync(
                It.IsAny<FixMessage>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    // ------------------------------------------------------------------ //
    // Partial-fill scenario
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task MatchAsync_BuyMoreThanAvailableSell_PartialFill()
    {
        var (svc, mockOrders, _, mockUsers, _) = BuildService();

        // Only 3 available on the sell side; buyer wants 8
        var sellOrder = MakeOpenSellOrder(price: 100m, qty: 3);
        MongoTestHelper.SetupFind(mockOrders, new[] { sellOrder });
        MongoTestHelper.SetupFind(
            mockUsers,
            new[] { new User { Id = "buyer",  WalletBalance = 2_000m } },
            new[] { new User { Id = "seller", WalletBalance = 0m } });

        var buyOrder = MakeIncomingBuyOrder(price: 100m, qty: 8);
        await svc.MatchAsync(buyOrder);

        Assert.Equal("PARTIAL", buyOrder.Status);
        Assert.Equal(5, buyOrder.RemainingQuantity);  // 8 - 3 = 5 remaining
    }

    // ------------------------------------------------------------------ //
    // Price-time priority (LIMIT order price check)
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task MatchAsync_BuyLimitBelowSellPrice_NoMatchOccurs()
    {
        var (svc, mockOrders, mockTrades, _, _) = BuildService();

        // Sell at 110; buyer only willing to pay 100
        var sellOrder = MakeOpenSellOrder(price: 110m, qty: 5);
        MongoTestHelper.SetupFind(mockOrders, new[] { sellOrder });

        var buyOrder = MakeIncomingBuyOrder(orderType: "LIMIT", price: 100m, qty: 5);
        await svc.MatchAsync(buyOrder);

        // No trade should have been inserted
        mockTrades.Verify(
            c => c.InsertOneAsync(
                It.IsAny<Trade>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task MatchAsync_SellLimitAboveBuyPrice_NoMatchOccurs()
    {
        var (svc, mockOrders, mockTrades, _, _) = BuildService();

        // Buy orders available at 80; seller wants at least 90
        var buyOrder = new Order
        {
            Id = "buy-passive",
            UserId = "buyer",
            InstrumentId = "ACME",
            Side = "BUY",
            OrderType = "LIMIT",
            Price = 80m,
            Quantity = 5,
            RemainingQuantity = 5,
            Status = "OPEN",
        };
        MongoTestHelper.SetupFind(mockOrders, new[] { buyOrder });

        var sellOrder = new Order
        {
            Id = "sell-active",
            UserId = "seller",
            InstrumentId = "ACME",
            Side = "SELL",
            OrderType = "LIMIT",
            Price = 90m,
            Quantity = 5,
            RemainingQuantity = 5,
            Status = "OPEN",
        };
        await svc.MatchAsync(sellOrder);

        mockTrades.Verify(
            c => c.InsertOneAsync(
                It.IsAny<Trade>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ------------------------------------------------------------------ //
    // No opposite orders
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task MatchAsync_NoOppositeOrders_StatusIsPartial()
    {
        var (svc, mockOrders, _, _, _) = BuildService();

        MongoTestHelper.SetupFind(mockOrders, Array.Empty<Order>());

        var buyOrder = MakeIncomingBuyOrder(price: 100m, qty: 5);
        await svc.MatchAsync(buyOrder);

        Assert.Equal("PARTIAL", buyOrder.Status);
        Assert.Equal(5, buyOrder.RemainingQuantity);
    }
}
