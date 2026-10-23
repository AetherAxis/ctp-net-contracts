using System;

namespace Ctp.Net;

/// <summary>
/// 节假日源（可选）。<see cref="CtpTradingCalendar"/> 只按「周一–周五 + 时段」判断，
/// 不含法定假日；需要精确的宿主自己实现本接口并传给
/// <see cref="CtpTradingCalendar.IsTradingTime(DateTime, ICtpHolidayCalendar?)"/>。
/// <para>本仓刻意不内置年度假期表：数据要每年维护，放进零依赖的契约层会变成长期负担。</para>
/// </summary>
public interface ICtpHolidayCalendar
{
    /// <summary>给定日期是否为休市日（节假日/调休）。</summary>
    bool IsHoliday(DateTime date);
}
