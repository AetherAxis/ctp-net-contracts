# Ctp.Net.Contracts

CTP 交易所**契约正典**（"协议宪法"）——三方共享（交易所 / 前置 / 客户端）的接缝类型集合，使接缝契约不依赖任何外部 SDK。零依赖叶子：任何人可引用，自身零 PackageReference。

## 组成

| 文件 | 说明 |
|---|---|
| `CtpModels.cs` | 领域 DTO（纯数据载体）：CtpLoginResult / Account / Position / Order / Trade / Tick / Instrument / OrderRequest 等 |
| `CtpEnums.cs` | byte 基枚举（Direction / OffsetFlag / OrderPriceType / TimeCondition 等），值精确镜像 CTP 协议编码规则 |
| `CtpExceptions.cs` | `CtpException` / `CtpBusinessException`——ErrorId 精度是 `ICtpSession` 接缝的既定契约 |
| `CtpClientContracts.cs` | 客户端会话形状类型（CtpLoginRequest / CtpMarketDataConnect / CtpConnectionStatus） |
| `ICtpSession.cs` | 客户端会话端口（交易 + 行情一体） |
| `ICtpConfigStore.cs` / `CtpConnectionConfig.cs` | 连接档案（前置地址/凭据存取）契约 |
| `CtpInstrument.cs` | 合约静态元数据 |
| `CtpTradingCalendar.cs` | CTP 交易时段表（日盘 + 夜盘宽口径并集，不含节假日） |

## 设计要点

- **namespace `Ctp.Net`**：三层命名空间分离——`Ctp.Net`（契约，本仓）/ `Ctp.Net.Core.*`（SDK wrapper，ctp-net-core）/ `Ctp.Net.Client`（客户端实现，ctp-net-client）——互不冲突
- **byte 基枚举**：值精确镜像 CTP 编码，适配层零转换成本
- **零依赖**：无 PackageReference，契约层不依赖任何 SDK / 框架包

## 消费方

ctp-net-front（服务端前置）、ctp-net-client（客户端 SDK 封装）、aether-trading（Lean 实盘链）。

## 来历

2026-10-19 拆自 Aether monorepo `business/CtpExchange/Acme.CtpExchange.Domain`（初名 ctp-contracts，namespace 沿用 `Acme.Ctp.Domain` 实现零 using 迁移）；2026-10-20 **契约正典化**——吸收客户端契约副本（aether-trading 的 `Acme.Ctp.Domain`），命名统一为 `Ctp.Net`。完整决策记录见工作区根 `MIGRATION.md`。
