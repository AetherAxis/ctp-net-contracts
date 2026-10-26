namespace Ctp.Net;

/// <summary>
/// 合约静态元数据（客户端可见面）。仿真柜台经 <c>queryInstruments</c> 暴露 host 上架的合约表
/// （「行情随柜台」的行情板数据源）；真实链路暂无合约查询（<c>CtpSessionService</c> 返回空表——
/// 行情板继续走证券库主力合约）。Domain 此前没有任何合约元数据——本 DTO 是第一个。
/// </summary>
public sealed record CtpInstrument
{
    /// <summary>合约代码（如 ag2610）。</summary>
    public string InstrumentId { get; init; } = "";

    /// <summary>合约中文名（2026-10-26 起服务端经 queryInstruments 携带——源在 ④ InstrumentCatalog 品种表；
    /// ax001 = Aether币价期货。注意：⑤ 的 CTP 线协议「查合约」应答帧尚无名称槽（未逆向）——
    /// RPC/进程内路径已带名，wire 路径补齐前客户端经 ⑤ 查到的 Name 为空）。</summary>
    public string Name { get; init; } = "";

    /// <summary>交易所（SHFE/DCE/CZCE/CFFEX/INE；合成合约为空串）。</summary>
    public string ExchangeId { get; init; } = "";

    /// <summary>合约乘数。</summary>
    public double VolumeMultiple { get; init; }

    /// <summary>最小变动价位。</summary>
    public double PriceTick { get; init; }

    /// <summary>昨结算价。</summary>
    public double PreSettlement { get; init; }

    /// <summary>基准价（合成兜底锚价）。</summary>
    public double BasePrice { get; init; }
}
