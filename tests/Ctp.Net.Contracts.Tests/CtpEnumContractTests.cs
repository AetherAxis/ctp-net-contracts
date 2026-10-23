namespace Ctp.Net.Contracts.Tests;

/// <summary>
/// 枚举值钉：契约层的每个 <c>byte</c> 都等于 CTP 协议里的 char 码。
/// 这些数字是三方（交易所/前置/客户端）共享的线上值——改动它们等于改协议，
/// 所以这里逐值断言字面量，任何「顺手重排枚举」都会立刻红。
/// </summary>
public class CtpEnumContractTests
{
    [Fact]
    public void 方向_买0卖1()
    {
        Assert.Equal((byte)'0', (byte)CtpDirection.Buy);
        Assert.Equal((byte)'1', (byte)CtpDirection.Sell);
    }

    [Fact]
    public void 开平_0开_1平_3平今_4平昨()
    {
        Assert.Equal((byte)'0', (byte)CtpOffsetFlag.Open);
        Assert.Equal((byte)'1', (byte)CtpOffsetFlag.Close);
        Assert.Equal((byte)'3', (byte)CtpOffsetFlag.CloseToday);
        Assert.Equal((byte)'4', (byte)CtpOffsetFlag.CloseYesterday);
    }

    [Fact]
    public void 价格类型_0市价_2限价()
    {
        Assert.Equal((byte)'0', (byte)CtpOrderPriceType.Market);
        Assert.Equal((byte)'2', (byte)CtpOrderPriceType.Limit);
    }

    [Fact]
    public void 有效期_IOC1_FOK2_GFD3_PostOnly4为本地扩展()
    {
        Assert.Equal((byte)'1', (byte)CtpTimeCondition.IOC);
        Assert.Equal((byte)'2', (byte)CtpTimeCondition.FOK);
        Assert.Equal((byte)'3', (byte)CtpTimeCondition.GFD);

        // '4' 不是真实 CTP 的 TC 值，是本仓的 crypto post-only/GTX 扩展；
        // 前置 codec 的 VolShape 白名单必须包含它，否则透传会被当成非法值。
        Assert.Equal((byte)'4', (byte)CtpTimeCondition.PostOnly);
    }

    [Fact]
    public void 持仓方向_1净_2多_3空()
    {
        Assert.Equal((byte)'1', (byte)CtpPosiDirection.Net);
        Assert.Equal((byte)'2', (byte)CtpPosiDirection.Long);
        Assert.Equal((byte)'3', (byte)CtpPosiDirection.Short);
    }

    [Fact]
    public void 投机套保_1投机_2套利_3套保()
    {
        Assert.Equal((byte)'1', (byte)CtpHedgeFlag.Speculation);
        Assert.Equal((byte)'2', (byte)CtpHedgeFlag.Arbitrage);
        Assert.Equal((byte)'3', (byte)CtpHedgeFlag.Hedge);
    }

    [Fact]
    public void 持仓日期_1今_2昨()
    {
        Assert.Equal((byte)'1', (byte)CtpPositionDate.Today);
        Assert.Equal((byte)'2', (byte)CtpPositionDate.History);
    }

    [Fact]
    public void 报单状态_逐值镜像THOST_FTDC_OST()
    {
        Assert.Equal((byte)'0', (byte)CtpOrderStatus.AllTraded);
        Assert.Equal((byte)'1', (byte)CtpOrderStatus.PartTradedQueueing);
        Assert.Equal((byte)'2', (byte)CtpOrderStatus.PartTradedNotQueueing);
        Assert.Equal((byte)'3', (byte)CtpOrderStatus.NoTradeQueueing);
        Assert.Equal((byte)'4', (byte)CtpOrderStatus.NoTradeNotQueueing);
        Assert.Equal((byte)'5', (byte)CtpOrderStatus.Canceled);
        Assert.Equal((byte)'a', (byte)CtpOrderStatus.Unknown);
        Assert.Equal((byte)'b', (byte)CtpOrderStatus.NotTouched);
        Assert.Equal((byte)'c', (byte)CtpOrderStatus.Touched);
    }

    [Theory]
    [InlineData('0', CtpOrderStatus.AllTraded)]
    [InlineData('1', CtpOrderStatus.PartTradedQueueing)]
    [InlineData('2', CtpOrderStatus.PartTradedNotQueueing)]
    [InlineData('3', CtpOrderStatus.NoTradeQueueing)]
    [InlineData('4', CtpOrderStatus.NoTradeNotQueueing)]
    [InlineData('5', CtpOrderStatus.Canceled)]
    [InlineData('a', CtpOrderStatus.Unknown)]
    [InlineData('b', CtpOrderStatus.NotTouched)]
    [InlineData('c', CtpOrderStatus.Touched)]
    public void 裸状态字节可翻成强类型(char raw, CtpOrderStatus expected)
        => Assert.Equal(expected, ((byte)raw).ToStatus());

    [Fact]
    public void 不认识的状态字节归Unknown_不抛()
    {
        // 柜台新增状态码时不能把回报线程炸掉——退化成 Unknown，原值仍在 OrderStatusRaw 里。
        Assert.Equal(CtpOrderStatus.Unknown, ((byte)'z').ToStatus());
        Assert.Equal(CtpOrderStatus.Unknown, ((byte)0).ToStatus());
    }

    [Theory]
    [InlineData(CtpOrderStatus.AllTraded, true)]
    [InlineData(CtpOrderStatus.Canceled, true)]
    [InlineData(CtpOrderStatus.NoTradeNotQueueing, true)]
    [InlineData(CtpOrderStatus.PartTradedNotQueueing, true)]
    [InlineData(CtpOrderStatus.NoTradeQueueing, false)]
    [InlineData(CtpOrderStatus.PartTradedQueueing, false)]
    [InlineData(CtpOrderStatus.NotTouched, false)]
    public void 终结态判定(CtpOrderStatus status, bool terminal)
        => Assert.Equal(terminal, status.IsTerminal());

    [Fact]
    public void 报单的强类型状态读自裸字节()
    {
        var order = new CtpOrder { OrderStatusRaw = (byte)'5' };
        Assert.Equal(CtpOrderStatus.Canceled, order.Status);
        Assert.True(order.Status.IsTerminal());
    }
}
