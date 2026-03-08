using TradingPlatform.Models;
using TradingPlatform.Repositories;
using TradingPlatform.Services;
using Moq;
using MongoDB.Driver;

namespace TradingPlatformAPI.Tests;

public class OrderServiceTests
{
    // ------------------------------------------------------------------ //
    // Helpers
    // ------------------------------------------------------------------ //

    private static (OrderService svc,
                    Mock<IMongoCollection<Order>> mockOrders,
                    Mock<IMongoCollection<Trade>> mockTrades,
                    Mock<IMongoCollection<User>> mockUsers,
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

        var orderRepo    = new OrderRepository(mockDb.Object);
        var tradeRepo    = new TradeRepository(mockDb.Object);
        var userRepo     = new UserRepository(mockDb.Object);
        var fixRepo      = new FixMessageRepository(mockDb.Object);
        var fixSvc       = new FixMessageService(fixRepo);
        var billingSvc   = new BillingService(userRepo);
        var matchingEngine = new MatchingEngineService(orderRepo, tradeRepo, billingSvc, fixSvc);
        var orderSvc     = new OrderService(orderRepo, matchingEngine);

        return (orderSvc, mockOrders, mockTrades, mockUsers, mockFix);
    }

    private static TradingPlatform.DTOs.OrderDTO MakeLimitBuyDto(
        string instrumentId = "ACME", decimal price = 100m, int qty = 5) =>
        new()
        {
            InstrumentId = instrumentId,
            Side = "BUY",
            OrderType = "LIMIT",
            Price = price,
            Quantity = qty,
        };

    // ------------------------------------------------------------------ //
    // PlaceOrder
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task PlaceOrder_CreatesOrderInDb()
    {
        var (svc, mockOrders, _, mockUsers, _) = BuildService();

        // No opposite orders to match against → empty list
        MongoTestHelper.SetupFind(mockOrders, Array.Empty<Order>());

        var dto = MakeLimitBuyDto();
        await svc.PlaceOrder("user-001", dto);

        // InsertOneAsync must have been called once (for the new order)
        mockOrders.Verify(
            c => c.InsertOneAsync(
                It.IsAny<Order>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PlaceOrder_ReturnedOrder_HasCorrectSideAndQty()
    {
        var (svc, mockOrders, _, _, _) = BuildService();

        MongoTestHelper.SetupFind(mockOrders, Array.Empty<Order>());

        var dto = MakeLimitBuyDto(qty: 7);
        var order = await svc.PlaceOrder("user-001", dto);

        Assert.Equal("BUY", order.Side);
        Assert.Equal(7, order.Quantity);
    }

    [Fact]
    public async Task PlaceOrder_NoOppositeOrders_StatusIsPartial()
    {
        var (svc, mockOrders, _, _, _) = BuildService();

        // Matching engine finds no SELL orders → incoming order stays PARTIAL
        MongoTestHelper.SetupFind(mockOrders, Array.Empty<Order>());

        var order = await svc.PlaceOrder("user-001", MakeLimitBuyDto());

        Assert.Equal("PARTIAL", order.Status);
    }

    [Fact]
    public async Task PlaceOrder_WithMatchingSellOrder_StatusIsFilled()
    {
        var (svc, mockOrders, _, mockUsers, _) = BuildService();

        var sellOrder = new Order
        {
            Id = "sell-001",
            UserId = "seller",
            InstrumentId = "ACME",
            Side = "SELL",
            OrderType = "LIMIT",
            Price = 100m,
            Quantity = 5,
            RemainingQuantity = 5,
            Status = "OPEN",
        };

        // First Find call: opposite orders (SELL side)
        // Second Find onwards: users for billing (buyer + seller)
        MongoTestHelper.SetupFind(mockOrders, new[] { sellOrder });
        MongoTestHelper.SetupFind(
            mockUsers,
            new[] { new User { Id = "user-001", WalletBalance = 1_000m } },
            new[] { new User { Id = "seller",   WalletBalance = 0m } });

        var dto = MakeLimitBuyDto(price: 100m, qty: 5);
        var order = await svc.PlaceOrder("user-001", dto);

        Assert.Equal("FILLED", order.Status);
        Assert.Equal(0, order.RemainingQuantity);
    }

    // ------------------------------------------------------------------ //
    // CancelOrder
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task CancelOrder_SetsStatusToCancelled()
    {
        var (svc, mockOrders, _, _, _) = BuildService();

        var existingOrder = new Order
        {
            Id = "order-999",
            UserId = "user-001",
            InstrumentId = "ACME",
            Side = "BUY",
            OrderType = "LIMIT",
            Price = 100m,
            Quantity = 3,
            RemainingQuantity = 3,
            Status = "OPEN",
        };

        MongoTestHelper.SetupFind(mockOrders, new[] { existingOrder });

        await svc.CancelOrder(existingOrder.Id);

        Assert.Equal("CANCELLED", existingOrder.Status);
    }

    [Fact]
    public async Task CancelOrder_NonExistentOrder_ThrowsException()
    {
        var (svc, mockOrders, _, _, _) = BuildService();

        // GetByIdAsync returns null (empty list → FirstOrDefault → null)
        MongoTestHelper.SetupFind(mockOrders, Array.Empty<Order>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.CancelOrder("non-existent-id"));
    }
}
