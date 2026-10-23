using System;

namespace Ctp.Net;

/// <summary>
/// 报单状态（镜像 CTP <c>THOST_FTDC_OST_*</c> 的 char 码）。线协议仍以
/// <see cref="CtpOrder.OrderStatusRaw"/> 的裸 byte 为准——本枚举只是给消费方一个可读、可穷举的视图，
/// 不改变任何已序列化的值。
/// </summary>
public enum CtpOrderStatus : byte
{
    /// <summary>'0' 全部成交。</summary>
    AllTraded = (byte)'0',

    /// <summary>'1' 部分成交，剩余仍在队列。</summary>
    PartTradedQueueing = (byte)'1',

    /// <summary>'2' 部分成交，剩余已不在队列（如 IOC 未成交部分被撤）。</summary>
    PartTradedNotQueueing = (byte)'2',

    /// <summary>'3' 未成交，仍在队列。</summary>
    NoTradeQueueing = (byte)'3',

    /// <summary>'4' 未成交，已不在队列。</summary>
    NoTradeNotQueueing = (byte)'4',

    /// <summary>'5' 已撤单。</summary>
    Canceled = (byte)'5',

    /// <summary>'a' 未知（柜台未给出）。</summary>
    Unknown = (byte)'a',

    /// <summary>'b' 条件单尚未触发。</summary>
    NotTouched = (byte)'b',

    /// <summary>'c' 条件单已触发。</summary>
    Touched = (byte)'c',
}

/// <summary>裸 byte → <see cref="CtpOrderStatus"/> 的显式映射（不用反射：报单回报是热点路径）。</summary>
public static class CtpOrderStatusExtensions
{
    /// <summary>把线协议里的状态字节翻成强类型；不认识的码归 <see cref="CtpOrderStatus.Unknown"/>。</summary>
    public static CtpOrderStatus ToStatus(this byte raw) => raw switch
    {
        (byte)'0' => CtpOrderStatus.AllTraded,
        (byte)'1' => CtpOrderStatus.PartTradedQueueing,
        (byte)'2' => CtpOrderStatus.PartTradedNotQueueing,
        (byte)'3' => CtpOrderStatus.NoTradeQueueing,
        (byte)'4' => CtpOrderStatus.NoTradeNotQueueing,
        (byte)'5' => CtpOrderStatus.Canceled,
        (byte)'a' => CtpOrderStatus.Unknown,
        (byte)'b' => CtpOrderStatus.NotTouched,
        (byte)'c' => CtpOrderStatus.Touched,
        _ => CtpOrderStatus.Unknown,
    };

    /// <summary>是否已终结（成交完毕或已撤单）——撤单/追单逻辑用它判断「不用再等」。</summary>
    public static bool IsTerminal(this CtpOrderStatus status) =>
        status is CtpOrderStatus.AllTraded or CtpOrderStatus.Canceled or CtpOrderStatus.NoTradeNotQueueing
            or CtpOrderStatus.PartTradedNotQueueing;
}
