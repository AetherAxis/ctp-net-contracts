using System;

namespace Ctp.Net;

/// <summary>
/// CTP 交易时段表（宽口径并集）。用于断线后判断该"盘中重连"还是"闭市等待开盘"。
/// 口径（并集，不按品种分级）：
/// <list type="bullet">
/// <item>日盘：周一–周五 09:00–10:15、10:30–11:30、13:30–15:00（10:15–10:15–10:30 休盘）。</item>
/// <item>夜盘：21:00–次日 02:30（周一–周五晚开盘；跨午夜到周二–周六凌晨 02:30 收盘）。</item>
/// </list>
/// <b>不含节假日表</b>——法定假日 IsTradingTime 仍返回 true，重连会失败并退避（不会崩）。
/// 后续若要精确，可加假日表覆盖。
/// </summary>
public static class CtpTradingCalendar
{
    /// <summary>判断给定本地时间是否处于 CTP 交易时段（宽口径并集，不含节假日）。</summary>
    public static bool IsTradingTime(DateTime now) => IsTradingTime(now, holidays: null);

    /// <summary>
    /// 带节假日源的版本：法定假日不交易。节假日表由宿主注入（本仓不维护年度数据，
    /// 也不改变无参重载的既有宽口径语义）。
    /// <para>跨午夜夜盘按「当前日历日」判断假日——节假日通常整段休市，够用；极端边界
    /// （节前最后一夜盘的后半段）以保守为准：判为不交易，重连会退避而不会崩。</para>
    /// </summary>
    public static bool IsTradingTime(DateTime now, ICtpHolidayCalendar? holidays)
    {
        var dt = now.Kind == DateTimeKind.Utc ? now.ToLocalTime() : now;
        if (holidays?.IsHoliday(dt.Date) == true)
            return false;
        var time = dt.TimeOfDay;
        var dow = dt.DayOfWeek;

        // 夜盘跨午夜：21:00–23:59（周一–周五晚）+ 00:00–02:30（周二–周六凌晨，承接前一夜盘）
        if (time >= TimeSpan.FromHours(21) && time <= TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59.999)))
            return dow >= DayOfWeek.Monday && dow <= DayOfWeek.Friday;
        if (time >= TimeSpan.Zero && time < TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(30)))
            return dow >= DayOfWeek.Tuesday && dow <= DayOfWeek.Saturday;

        // 日盘：09:00–10:15、10:30–11:30、13:30–15:00（周一–周五）
        if (dow < DayOfWeek.Monday || dow > DayOfWeek.Friday)
            return false;
        if (time >= TimeSpan.FromHours(9) && time < TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(15)))
            return true;
        if (time >= TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(30)) && time < TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30)))
            return true;
        if (time >= TimeSpan.FromHours(13).Add(TimeSpan.FromMinutes(30)) && time < TimeSpan.FromHours(15))
            return true;

        return false;
    }
}
