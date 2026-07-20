# 仿 HOK 帧同步对战 Demo

> 基于 **Unity 2022.3 LTS + TEngine 框架** 开发的帧同步 MOBA 实时对战 Demo,覆盖登录 → 大厅 → 匹配 → 选英雄 → 加载 → 对战 → 结算的完整流程,战斗采用 **帧同步 + 定点数运算** 保证多端确定性。
>
> 🔧 **项目状态:开发中。** MOBA 流程骨架与帧同步战斗核心已跑通,Buff/技能体系与英雄内容持续完善。

---

## 项目简介

仿王者荣耀(HOK)的 3D MOBA 实时对战 Demo。玩家经登录、匹配、选英雄进入对战,战斗以帧同步驱动,逻辑层全定点数运算,确保客户端/服务端计算一致性。

---

## 技术栈

| 类别 | 选型 |
|---|---|
| 引擎 | Unity 2022.3 LTS |
| 框架 | TEngine(模块系统 / UI 生命周期 / 事件 / 资源 / 流程状态机) |
| 热更 | HybridCLR(GameProto + GameLogic 热更,AOT 元数据补充) |
| 资源 | YooAsset(打包 / 加载 / 版本热更) |
| 异步 | UniTask |
| 配置 | Luban(8 张表:xlsx → bytes + cs) |
| 帧同步 | PEMath 定点数 + PEPhysx 定点数物理(预编译 dll) |
| 网络 | KCP / KCPNet(UDP 可靠传输) |
| 协议 | HOKProtocol(客户端 / 服务端共享) |

---

## 程序集划分

| 程序集 | 职责 | 热更 |
|---|---|---|
| `Launcher` | 冷启动入口 `GameEntry` + 启动流程 Procedure | 否 |
| `TEngine.Runtime` | 框架运行时(模块 / 事件 / UI / 资源) | 否 |
| `GameProto` | Luban 生成的配置代码(Tables / TbXxx) | 是 |
| `GameLogic` | 业务核心(战斗 / UI / 网络 / 系统) | 是 |

---

## 核心技术

### 1. 帧同步定点数战斗
- 战斗逻辑层**不依赖 Unity API**。`LogicUnit` 基类用 `PEVector3` / `PEInt`(PEMath 定点数)存储位置、方向、速度,保证多端确定性
- `FightMgr` 管理战斗单元,`Tick()` 遍历逻辑单元驱动 `LogicTick()`
- **客户端跟随服务器模型**:逻辑 Tick 由服务器下发的 `NtfOpKey` 操作帧触发,而非本地每帧自驱
- 战斗实体:`Hero` / `Soldier` / `Tower`(均继承 `MainLogicUnit`,用 `partial` 拆分为 Move / Attrs / Skill)、`Skill`、`Buff`、`Bullet`
- 定点数物理 `PEPhysx` 驱动碰撞(如 `PECylinderCollider`)

### 2. 战斗逻辑 / 表现分离
- `Battle/Logic/`(纯 C#,可脱离 Unity)与 `Battle/View/`(MonoBehaviour)**目录级隔离**
- `Hero`(纯逻辑) + `HeroView`(表现):逻辑层持有表现引用,表现层只读逻辑状态,单向数据流
- 表现层 `ViewUnit.Update()` 每帧插值平滑 + 预测;逻辑层按网络帧 Tick —— 渲染流畅、逻辑确定

### 3. GameEvent 事件驱动
- 跨层通信用 `[EventInterface]` 接口事件,TEngine **源生成器在构建期**自动生成事件 ID 与代理(零手写 ID、零运行时反射)
- **19 个事件接口**,分 Battle / Lobby / Player / Login / Network / UI 六组
- 网络消息路由用 string-keyed `GameEvent`(**非 switch 硬编码**),`NetMessageBindings` 集中订阅 10 个 CMD
- UI 内部用 `AddUIEvent`,随 Widget 生命周期自动清理

### 4. HybridCLR 热更 + 双段流程状态机
- 冷启动 `GameEntry` → 非热更 Procedure FSM(资源初始化 / 下载 / 程序集加载)→ 反射调用热更入口 `GameApp.Entrance`
- 热更侧新建第二段 GameFlow FSM:`Login → Lobby → Match → Select → Load → Battle`
- AOT 元数据补充 7 个 dll(mscorlib / System / TEngine.Runtime / UniTask / YooAsset 等)

### 5. Luban 配置驱动
- **8 张表**(Unit / Hero / Map / Skill / Buff / TargetRule / ClientAudio / ClientSetting):xlsx 源 → `.bytes`(`AssetRaw/Configs/bytes`) + `.cs`(`GameProto`)
- 多态 Buff 配置(Arthur / 后羿系列 7 种 `BuffCfg` 子类由 Luban 生成)
- `GameServices.Config` 统一出口,业务层只接触 `UnitCfg` / `SkillCfg` 等 DTO;配置 `float → PEInt`、`Vector3 → PEVector3` 自动转换

### 6. 网络层
- KCP 可靠传输(`KCPNet<ClientSession, HOKMsg>`),`NetSvc` 单例管理连接与收发主循环
- `ClientSession` 回调入队 → 主线程 `NetSvc` 出队 → `NetMsg.Route` 经 GameEvent 广播
- `GMService` 本地模拟服务器收包,**无需启动真服务端**即可跑通完整 MOBA 流程

---

## 模块与 UI

**`GameLogic` 程序集(111 个 .cs):**

| 模块 | 文件数 | 说明 |
|---|---|---|
| `Battle` | 22 | 帧同步战斗(Logic / View 分离 + FightMgr + MapRoot) |
| `UI` | 27 | UIWindow + UIWidget(含代码生成 `.g.cs`) |
| `IEvent` | 19 | 事件接口定义 |
| `Module` | 12 | UIModule 等自定义模块 |
| `Services` | 8 | NetSvc / ConfigService / GMService / GameServices 等 |
| `Procedure` | 7 | 热更游戏流程状态机 |
| `Systems` | 4 | Login / Lobby / Battle / GameSystem |
| `States` | 3 | Battle / Lobby / Session 状态 |

**UI 窗口(10 个业务窗口):** `LoginUI` / `LobbyUI` / `MatchUI` / `SelectUI` / `LoadUI` / `PlayUI`(战斗 HUD) / `HPUI`(血条层) / `StartUI` / `ResultUI` / `TipsUI`

**UIWidget(3 个):** `HPItemWidget`(单体血条) / `SkillItem`(技能槽) / `ItemHero`(英雄选择项)

---

## 开发状态

**已实现**
- 完整 MOBA 流程骨架(登录 / 大厅 / 匹配 / 选英雄 / 加载 / 对战 / 结算)
- 帧同步战斗核心:`Hero` / `Soldier` / `Tower` / `Skill` / `Buff` / `Bullet` 逻辑单元 + 表现层
- KCP 网络 + 事件路由 + GM 本地模拟
- Luban 配置管线(8 张表)+ HybridCLR 热更双段流程
- 10 个 UI 窗口 + 战斗 HUD / 血条 / 技能槽

**开发中 / 待完善**
- Buff 体系:框架已搭(`BuffRegistry` 注册机制),已实现移速 / 治疗 2 种,Arthur / 后羿等 BuffType 配置就绪、运行时子类补全中
- Skill 完善(部分音效 / 子弹 / Buff 附加标记为 todo)
- 定点数逻辑定时器 `LogicTimer` 已定义、待启用
- 更多英雄内容、联机实测

---

## 目录结构

```
TEngine/
├── UnityProject/Assets/
│   ├── GameScripts/
│   │   ├── GameEntry.cs            # 冷启动入口
│   │   ├── Procedure/              # 非热更启动流程(13 节点)
│   │   └── HotFix/
│   │       ├── GameProto/          # Luban 配置代码(热更)
│   │       └── GameLogic/          # 业务核心(热更)
│   │           ├── Battle/         # 帧同步战斗(Logic / View)
│   │           ├── IEvent/         # 事件接口
│   │           ├── UI/             # UIWindow + UIWidget
│   │           ├── Services/       # 网络 / 配置 / GM 服务
│   │           ├── Systems/        # 业务系统
│   │           └── Procedure/      # 热更游戏流程(7 节点)
│   ├── TEngine/                    # 框架
│   └── AssetRaw/Configs/bytes/     # Luban 配置产物
├── Configs/GameConfig/             # Luban xlsx 源 + 生成脚本
└── Tools/                          # Luban.exe + GameEvent 源生成器
```

---

## 运行

1. Unity 2022.3 LTS 打开 `TEngine/UnityProject`
2. 首次打开自动重建 `Library/`;YooAsset 资源需在 Editor 内 Build
3. GM 模式可直接跑通完整流程(无需启动服务端)

---

## 致谢

- **[TEngine](https://github.com/Alex-Rachel/TEngine)** — Unity 游戏框架,本项目的基础设施

---

## 作者

- **hdt1** · 3144419274@qq.com

---

> 本项目用于学习与求职展示,所有英雄 / 技能 / 美术资源均为仿制 Demo 内容,不用于商业用途。
