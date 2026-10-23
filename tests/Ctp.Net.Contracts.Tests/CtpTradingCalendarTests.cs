namespace Ctp.Net.Contracts.Tests;

/// <summary>
/// 交易时段表（宽口径并集）。时间点全部用具体日期构造——2026-10-19 周一 / 23 周五 / 24 周六。
/// 这些断言同时是「口径文档」：改动时段边界会立刻红。
/// </summary>
public class CtpTradingCalendarTests
{
    private static DateTime At(int day, int hour, int minute, int second = 0)
        => new(2026, 10, day, hour, minute, second, DateTimeKind.Unspecified);

    [Theory]
    [InlineData(20, 9, 0)]      // 日盘开
    [InlineData(20, 10, 14)]    // 第一节末
    [InlineData(20, 10, 30)]    // 第二节开
    [InlineData(20, 13, 30)]    // 午盘开
    [InlineData(20, 14, 59)]    // 收盘前
    public void 日盘时段内_为交易时间(int day, int h, int m)
        => Assert.True(CtpTradingCalendar.IsTradingTime(At(day, h, m)));

    [Theory]
    [InlineData(20, 10, 15)]    // 10:15 起休盘（右开）
    [InlineData(20, 10, 29)]
    [InlineData(20, 11, 30)]    // 上午收盘（右开）
    [InlineData(20, 12, 0)]     // 午休
    [InlineData(20, 15, 0)]     // 日盘收盘（右开）
    [InlineData(20, 16, 0)]
    [InlineData(20, 8, 59)]
    public void 日盘休盘与收盘后_不是交易时间(int day, int h, int m)
        => Assert.False(CtpTradingCalendar.IsTradingTime(At(day, h, m)));

    [Fact]
    public void 周六日盘不交易()
        => Assert.False(CtpTradingCalendar.IsTradingTime(At(24, 10, 0)));

    [Fact]
    public void 周日日盘不交易()
        => Assert.False(CtpTradingCalendar.IsTradingTime(At(25, 10, 0)));

    [Fact]
    public void 周五夜盘开_次日凌晨仍算交易()
    {
        Assert.True(CtpTradingCalendar.IsTradingTime(At(23, 21, 30)));   // 周五 21:30
        Assert.True(CtpTradingCalendar.IsTradingTime(At(24, 1, 0)));     // 周六 01:00（承接周五夜盘）
    }

    [Theory]
    [InlineData(20, 2, 30)]     // 周二 02:30 整 = 收盘（右开）
    [InlineData(24, 2, 30)]     // 周六 02:30 整 = 收盘
    public void 夜盘收盘边界_不是交易时间(int day, int h, int m)
        => Assert.False(CtpTradingCalendar.IsTradingTime(At(day, h, m)));

    [Fact]
    public void 夜盘收盘前仍是交易时间()
        => Assert.True(CtpTradingCalendar.IsTradingTime(At(24, 2, 29)));

    [Fact]
    public void 周日夜里没有夜盘()
        => Assert.False(CtpTradingCalendar.IsTradingTime(At(25, 21, 30)));

    [Fact]
    public void 周一夜里没有夜盘_因为承接的是周日()
        => Assert.False(CtpTradingCalendar.IsTradingTime(At(19, 1, 0)));

    [Fact]
    public void 注入节假日源后_假日不交易()
    {
        var holidays = new FixedHolidays(new DateTime(2026, 10, 20));
        // 无节假日源时周二 10:00 是交易日；注入后应判为休市。
        Assert.True(CtpTradingCalendar.IsTradingTime(At(20, 10, 0)));
        Assert.False(CtpTradingCalendar.IsTradingTime(At(20, 10, 0), holidays));
        // 非假日不受影响。
        Assert.True(CtpTradingCalendar.IsTradingTime(At(21, 10, 0), holidays));
    }

    [Fact]
    public void UTC时间先转本地再判断()
    {
        var utc = new DateTime(2026, 10, 20, 2, 0, 0, DateTimeKind.Utc);   // 本地 10:00（CST）
        var local = utc.ToLocalTime();
        Assert.Equal(CtpTradingCalendar.IsTradingTime(local), CtpTradingCalendar.IsTradingTime(utc));
    }

    private sealed class FixedHolidays(params DateTime[] days) : ICtpHolidayCalendar
    {
        public bool IsHoliday(DateTime date) => days.Contains(date.Date);
    }
}
