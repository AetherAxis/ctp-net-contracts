# Ctp.Net.Contracts 设计说明

> 本文件写「怎么设计 / 为什么这么设计 / 还能往哪升级」。工作区边界与迁移决策以根 `MIGRATION.md` 为准。

## 1. 定位与边界

三方共享的**契约正典**：只放 DTO / 枚举 / 接口 / 异常 / 日历，零 `PackageReference`，只引 BCL，是依赖图叶子。消费方为 ctp-net-front、ctp-net-client、aether-trading。

边界：不放编解码、I/O、业务行为；签名只用 `Ctp.Net` 类型，SDK 类型不得出现，也不反向引用消费方。

## 2. 项目组成

| 文件 | 职责 | 关键类型 | 出向依赖 |
|---|---|---|---|
| `CtpModels.cs` | 交易域 DTO | 8 个 DTO（LoginResult / Order / Trade / Tick 等） | 无 |
| `CtpEnums.cs` | byte 协议枚举 | 7 个枚举（Direction / OffsetFlag / TimeCondition 等） | 无 |
| `CtpExceptions.cs` | 领域异常 | CtpException / CtpBusinessException | 无 |
| `CtpClientContracts.cs` | 客户端会话形状 | LoginRequest / MarketDataConnect / ConnectionStatus | 无 |
| `ICtpSession.cs` | 交易 + 行情端口 | ICtpSession | 无 |
| `ICtpConfigStore.cs` | 连接档案存取 | Get / Save / Delete 等 5 方法 | 无 |
| `CtpConnectionConfig.cs` | 档案模型 | CtpConnectionConfig | 无 |
| `CtpInstrument.cs` | 合约元数据 | CtpInstrument | 无 |
| `CtpTradingCalendar.cs` | 时段判断 | IsTradingTime | 无 |

单项目 `net10.0`；程序集名 `ctp-net-contracts`，根命名空间 `Ctp.Net`；CPM 为空占位。

## 3. 关键设计决策

### 3.1 零依赖叶子
- **决策**：不引任何 NuGet，连日志 / JSON / 校验库都不进契约层。
- **理由**：三方同时引用，任何传递依赖都会变成版本冲突。
- **代价**：校验只能靠 `required`，新增能力须自写且克制。

### 3.2 byte 枚举镜像协议编码
- **决策**：枚举底层为 `byte`，值取协议 char 码（`Buy='0'`、`CloseToday='3'`、`GFD='3'`）。
- **理由**：适配层零转换，wire 码与领域值一一对应；`PostOnly='4'` 明确标为本地扩展值。
- **代价**：失去自解释序号；非法 byte 可强转进来；定错会同时污染三方。

### 3.3 异常类型承载 ErrorId 精度
- **决策**：`CtpBusinessException(ErrorId, ErrorMsg)` 原样带码与消息穿过 `ICtpSession` 接缝。
- **理由**：ErrorId 是既定契约。§16.1 实证：传 `ex.Message` 包装文本经 GBK 截断会破坏 native 帧，后续操作全超时。
- **代价**：各适配层必须显式映射，字段不可随意增删。

### 3.4 接缝只暴露本命名空间类型
- **决策**：`ICtpSession` 入参、返回、事件全部用本仓 DTO。
- **理由**：契约与实现解耦，客户端可被 RPC 实现替换；SDK 升级不波及三方。
- **代价**：SDK 字段变化需手工映射，漏字段是主要缺陷源。

### 3.5 交易日历宽口径并集
- **决策**：日盘 09:00–10:15 / 10:30–11:30 / 13:30–15:00，夜盘 21:00–次日 02:30，不含节假日。
- **理由**：只服务「盘中重连 or 闭市等待」；宽口径在假日最多无效重连并退避，不会崩。
- **代价**：法定假日仍返回 true；品种级夜盘差异未建模。

## 4. 运行与验证

- 构建 / CI：`dotnet build Ctp.Net.Contracts.slnx -c Release`（SDK 10.0.x，`net10.0`）；CI 在 ubuntu-latest 上 checkout 到 `ctp-net-contracts/`，仅跑 Release 构建，无 Test 步。
- 本仓无测试项目（0 项）；`MIGRATION` ③ 行同，测试暂由 Exchange 侧覆盖。回归由消费方承担：ctp-net-client 24、ctp-net-front 73、aether-exchange 138 例。
- 结论：当前只能证明能编译，不能证明与 SDK 协议码一致。

## 5. 升级记录（2026-10-24）

**无变更。** `git status --short` 与 `git diff` 均为空，HEAD 仍为 `b3695f6`。

原因：零包依赖、零工具链耦合，CPM 为空占位，外部升级不传导至此。协议正典不随工具链升级而动，只应由协议码或接缝契约变化驱动。

## 6. 待升级项

| # | 优先级 | 项目 | 现状证据 | 建议 | 代价 / 风险 |
|---|---|---|---|---|---|
| 1 | P0 | 缺与 SDK 映射的一致性测试 | 无 tests 项目；CI 仅 Build；MIGRATION ③ 行同 | 断言枚举值 == SDK char 码，核对字段覆盖 | 需引用 ctp-net-core，CI 要 checkout 兄弟仓 |
| 2 | P1 | CI 无 Test 步 | `ci.yml` 只有 `dotnet build` | #1 落地后加 `dotnet test` | 增加 checkout 时间 |
| 3 | P1 | 日历无节假日表 | `CtpTradingCalendar.cs` 注释明示；MIGRATION ③ 行 | 引入可注入假日表，宽口径兜底 | 需维护年度数据 |
| 4 | P1 | PostOnly '4' 白名单靠人工 | `CtpEnums.cs` 注明 front `VolShape` 须含 '4'；§16.1 有同类回归 | 一致性测试加白名单断言 | 跨仓耦合 |
| 5 | P2 | 撮合状态是裸 byte | `CtpOrder.OrderStatusRaw` 无枚举、无文档 | 增补 `CtpOrderStatus` 镜像 THOST_FTDC_OST_* | 码多易错 |
| 6 | P2 | 金额 / 价格全用 double | Account / Position / Tick / Order 均 `double` | 记录精度策略，或资金字段评估 decimal | 与 SDK double 转换可能有舍入差异 |

## 7. 本轮升级落地（2026-10-24）

- **契约钉子测试**（原为 0 例 → 57 例）：新建 `tests/Ctp.Net.Contracts.Tests`（纯托管、零 native、零兄弟仓，CI 加 Test 步）。覆盖：每个枚举 byte == CTP char 码、`ToStatus()` 全表 + 未知码→`Unknown`、`IsTerminal`、交易时段表的日盘/夜盘/周末/节假日注入/UTC 换算边界、`CtpBusinessException` 的 ErrorId 精度与消息格式。
- **契约补齐**：新增 `CtpOrderStatus`（镜像 `THOST_FTDC_OST_*`）+ `ToStatus()`/`IsTerminal()`，`CtpOrder.Status` 供消费方用强类型视图（`OrderStatusRaw` 线协议不变）。
- **可注入节假日**：`CtpTradingCalendar.IsTradingTime(DateTime, ICtpHolidayCalendar?)` 重载 + `ICtpHolidayCalendar`——原来只能吃内置表，节假日无法替换。
- **持仓成本**：`CtpPosition.PositionCost`（原生 `PositionCost`，= Σ 开仓价×手数×乘数）——上层不再只能拿最新价冒充开仓价。
