using TradingPlatform.Models;
using TradingPlatform.Repositories;
using TradingPlatform.Services;
using Moq;
using MongoDB.Driver;

namespace TradingPlatformAPI.Tests;

public class FixMessageServiceTests
{
    // ------------------------------------------------------------------ //
    // Helpers
    // ------------------------------------------------------------------ //

    private static FixMessageService BuildService(
        out Mock<IMongoCollection<FixMessage>> mockCollection)
    {
        mockCollection = MongoTestHelper.CreateCollection<FixMessage>();
        var mockDb = MongoTestHelper.RegisterCollection<FixMessage>(null, "FixMessages", mockCollection);
        var repo = new FixMessageRepository(mockDb.Object);
        return new FixMessageService(repo);
    }

    private static (Order order, Trade trade) MakeFullyFilledBuyOrderAndTrade()
    {
        var order = new Order
        {
            Id = "order-buy-001",
            UserId = "user-001",
            InstrumentId = "ACME",
            Side = "BUY",
            OrderType = "LIMIT",
            Price = 105.00m,
            Quantity = 10,
            RemainingQuantity = 0,  // fully filled
            Status = "FILLED",
        };

        var trade = new Trade
        {
            Id = "trade-001",
            BuyOrderId = order.Id,
            SellOrderId = "order-sell-001",
            InstrumentId = "ACME",
            Price = 103.50m,
            Quantity = 10,
        };

        return (order, trade);
    }

    // ------------------------------------------------------------------ //
    // CreateExecutionReport – field correctness
    // ------------------------------------------------------------------ //

    [Fact]
    public void CreateExecutionReport_FullyFilledBuyOrder_SetsExecTypeToFill()
    {
        var svc = BuildService(out _);
        var (order, trade) = MakeFullyFilledBuyOrderAndTrade();

        var msg = svc.CreateExecutionReport(order, trade, "1");

        Assert.Equal("F", msg.ExecType);
        Assert.Equal("2", msg.OrdStatus);
    }

    [Fact]
    public void CreateExecutionReport_PartialFillSellOrder_SetsExecTypeToPartialFill()
    {
        var svc = BuildService(out _);
        var order = new Order
        {
            Id = "order-sell-002",
            UserId = "user-002",
            InstrumentId = "BRACBANK",
            Side = "SELL",
            OrderType = "LIMIT",
            Price = 50.00m,
            Quantity = 20,
            RemainingQuantity = 8,  // partially filled (12 executed)
        };
        var trade = new Trade
        {
            Id = "trade-002",
            BuyOrderId = "order-buy-002",
            SellOrderId = order.Id,
            InstrumentId = "BRACBANK",
            Price = 50.00m,
            Quantity = 12,
        };

        var msg = svc.CreateExecutionReport(order, trade, "2");

        Assert.Equal("1", msg.ExecType);  // PartialFill
        Assert.Equal("1", msg.OrdStatus); // PartiallyFilled
    }

    [Fact]
    public void CreateExecutionReport_ComputesCumQtyAndLeavesQtyCorrectly()
    {
        var svc = BuildService(out _);
        var order = new Order
        {
            Id = "order-003",
            UserId = "user-003",
            InstrumentId = "ACME",
            Side = "BUY",
            OrderType = "LIMIT",
            Price = 100.00m,
            Quantity = 30,
            RemainingQuantity = 10,  // 20 filled so far
        };
        var trade = new Trade
        {
            Id = "trade-003",
            BuyOrderId = order.Id,
            SellOrderId = "order-sell-003",
            InstrumentId = "ACME",
            Price = 99.00m,
            Quantity = 20,
        };

        var msg = svc.CreateExecutionReport(order, trade, "1");

        Assert.Equal(20, msg.CumQty);
        Assert.Equal(10, msg.LeavesQty);
    }

    [Fact]
    public void CreateExecutionReport_SetsLastPxAndLastQtyFromTrade()
    {
        var svc = BuildService(out _);
        var (order, trade) = MakeFullyFilledBuyOrderAndTrade();

        var msg = svc.CreateExecutionReport(order, trade, "1");

        Assert.Equal(trade.Price, msg.LastPx);
        Assert.Equal(trade.Quantity, msg.LastQty);
    }

    [Fact]
    public void CreateExecutionReport_MarketOrder_UsesTradePriceAsMessagePrice()
    {
        var svc = BuildService(out _);
        var order = new Order
        {
            Id = "order-market-001",
            UserId = "user-004",
            InstrumentId = "ACME",
            Side = "BUY",
            OrderType = "MARKET",
            Price = null,  // MARKET orders have no limit price
            Quantity = 5,
            RemainingQuantity = 0,
        };
        var trade = new Trade
        {
            Id = "trade-market-001",
            BuyOrderId = order.Id,
            SellOrderId = "order-sell-004",
            InstrumentId = "ACME",
            Price = 101.00m,
            Quantity = 5,
        };

        var msg = svc.CreateExecutionReport(order, trade, "1");

        Assert.Equal(trade.Price, msg.Price);
    }

    // ------------------------------------------------------------------ //
    // RawMessage – FIX wire-format checks
    // ------------------------------------------------------------------ //

    [Fact]
    public void CreateExecutionReport_RawMessageStartsWithBeginString()
    {
        var svc = BuildService(out _);
        var (order, trade) = MakeFullyFilledBuyOrderAndTrade();

        var msg = svc.CreateExecutionReport(order, trade, "1");

        Assert.StartsWith("8=FIX.4.2", msg.RawMessage);
    }

    [Fact]
    public void CreateExecutionReport_RawMessageContainsRequiredTags()
    {
        var svc = BuildService(out _);
        var (order, trade) = MakeFullyFilledBuyOrderAndTrade();

        var msg = svc.CreateExecutionReport(order, trade, "1");

        // Verify presence of key FIX tags
        Assert.Contains("35=8",   msg.RawMessage);  // MsgType = ExecutionReport
        Assert.Contains("150=",   msg.RawMessage);  // ExecType
        Assert.Contains("39=",    msg.RawMessage);  // OrdStatus
        Assert.Contains("55=",    msg.RawMessage);  // Symbol
        Assert.Contains("54=",    msg.RawMessage);  // Side
        Assert.Contains("31=",    msg.RawMessage);  // LastPx
        Assert.Contains("32=",    msg.RawMessage);  // LastQty
        Assert.Contains("151=",   msg.RawMessage);  // LeavesQty
        Assert.Contains("14=",    msg.RawMessage);  // CumQty
        Assert.Contains("10=",    msg.RawMessage);  // Checksum
    }

    [Fact]
    public void CreateExecutionReport_RawMessageEndsWithChecksumField()
    {
        var svc = BuildService(out _);
        var (order, trade) = MakeFullyFilledBuyOrderAndTrade();

        var msg = svc.CreateExecutionReport(order, trade, "1");

        // The checksum field is always the last field and must be exactly 3 digits
        // followed by a SOH delimiter
        Assert.Matches(@"10=\d{3}\u0001$", msg.RawMessage);
    }

    // ------------------------------------------------------------------ //
    // RecordExecutionReportsAsync – persistence
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task RecordExecutionReportsAsync_InsertsOneMsgPerSide()
    {
        var svc = BuildService(out var mockCollection);

        var buyOrder = new Order
        {
            Id = "buy-001", UserId = "u1", InstrumentId = "ACME",
            Side = "BUY", OrderType = "LIMIT", Price = 100m,
            Quantity = 5, RemainingQuantity = 0,
        };
        var sellOrder = new Order
        {
            Id = "sell-001", UserId = "u2", InstrumentId = "ACME",
            Side = "SELL", OrderType = "LIMIT", Price = 100m,
            Quantity = 5, RemainingQuantity = 0,
        };
        var trade = new Trade
        {
            Id = "t-001", BuyOrderId = buyOrder.Id, SellOrderId = sellOrder.Id,
            InstrumentId = "ACME", Price = 100m, Quantity = 5,
        };

        await svc.RecordExecutionReportsAsync(buyOrder, sellOrder, trade);

        // InsertOneAsync must have been called exactly twice (BUY + SELL sides)
        mockCollection.Verify(
            c => c.InsertOneAsync(
                It.IsAny<FixMessage>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}
