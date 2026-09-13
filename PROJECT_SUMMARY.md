# 风见幽香（KazamiYuka）Mod 项目总结

> 生成日期：2026/06/01  
> 这是一个基于 **RitsuLib** 框架开发的《杀戮尖塔2》（Slay the Spire 2）角色 Mod。  
> 核心玩法围绕**召唤向日葵**与**阳光资源系统**展开。

---

## 一、项目概览

| 项目 | 内容 |
|------|------|
| Mod ID | `KazamiYuka` |
| 角色名称 | 风见幽香 |
| 框架依赖 | `STS2-RitsuLib` |
| 主题色 | `#00cc00` 纯绿 |
| 初始血量 | 84 |
| 初始金币 | 99 |
| 性别 | 女性（Feminine）|

---

## 二、已完成内容清单

### 2.1 角色系统

| 文件 | 说明 |
|------|------|
| `Scripts/Entry.cs` | Mod 入口，初始化 Harmony、脚本注册、阳光图标预加载 |
| `Scripts/KazamiYukaCharacter.cs` | 角色主定义（颜色、血量、资产路径、动画延迟等）|
| `Scripts/KazamiYukaPools.cs` | 卡池 / 遗物池 / 药水池定义（绿色主题、自定义能量图标）|
| `Scripts/KazamiYukaKeywords.cs` | 注册两个自定义关键词：`成长(Grow)`、`烈阳(FierceSun)` |

**已配置的场景资产：**
- `kazamiyuka_character.tscn` — 战斗角色模型
- `kazamiyuka_energy_counter.tscn` — 能量计数器
- `kazamiyuka_character_merchant.tscn` — 商店角色
- `kazamiyuka_character_rest_site.tscn` — 篝火休息角色
- `kazamiyuka_bg.tscn` — 角色选择背景
- `icon.tscn` — 角色头像

---

### 2.2 卡牌（共 25 张）

#### 基础卡（Starter）

| 卡牌 | 费用 | 类型 | 稀有度 | 数量 | 效果概要 |
|------|------|------|--------|------|----------|
| **打击** (Strike_Yuka) | 1 | 攻击 | 基础 | x4 | 造成 6/9 点伤害。Tag: `Strike` |
| **防御** (Defend_Yuka) | 1 | 技能 | 基础 | x4 | 获得 5/8 点格挡。Tag: `Defend` |
| **播下种子** (SowSeeds_Yuka) | 1→0 | 技能 | 基础 | x1 | `[成长]` 召唤一朵向日葵，给予 4 点最大生命。升级后 0 费 |
| **阳光照耀** (Sunshine_Yuka) | 1 | 攻击 | 基础 | x1 | `[烈阳]` 选择向日葵，消耗阳光为其回血+加生命上限，随后对最低血量敌人造成向日葵生命值的伤害。升级后烈阳 5→7 |

#### 普通卡（Common）

| 卡牌 | 费用 | 类型 | 效果概要 |
|------|------|------|----------|
| **高照** (HighIllumination_Yuka) | 1 | 技能 | 使所有向日葵在回合结束时对最低血量敌人造成 4/6 点伤害 |
| **阳光收集** (SunlightCollection_Yuka) | 0 | 技能 | 获得 3/5 点阳光。`[消耗]` |
| **光合作用** (Photosynthesis_Yuka) | 1 | 技能 | 为一朵向日葵恢复 5/8 点生命。仅可选向日葵 |
| **阳伞招架** (UmbrellaParry_Yuka) | 1 | 技能 | 获得 8 点格挡，获得 1/2 点阳光 |
| **阳光打击** (SunlightStrike_Yuka) | 1 | 攻击 | 造成 10/14 点伤害，消耗 1 点阳光 |
| **花丛护身** (FlowerBedProtection_Yuka) | 0 | 技能 | 选择向日葵，使其 +4/6 最大生命，获得 `守护` 状态。消耗 1 阳光。仅可选向日葵 |
| **团结** (Unity_Yuka) | 2 | 技能 | `[成长]` 召唤向日葵，基础 2/3 点，场上每有一朵向日葵数值翻倍 |
| **遮挡** (Shading_Yuka) | 1 | 技能 | 最多消耗 2 点阳光，每消耗 1 点获得 7/10 点格挡 |
| **伞的技艺** (UmbrellaTechnique_Yuka) | 1 | 攻击 | 造成 5/6 点伤害；初始攻击 1 次，场上每有一朵向日葵就额外攻击 1 次 |

#### 罕见卡（Uncommon）

| 卡牌 | 费用 | 类型 | 效果概要 |
|------|------|------|----------|
| **花的报恩** (FlowerGratitude_Yuka) | 2→1 | 能力 | 每当向日葵回血或加生命上限时，抽 1 张卡。消耗 10 阳光 |
| **下午茶** (AfternoonTea_Yuka) | 1 | 技能 | 本回合内，每当阳光变化时，随机/所有向日葵 +1 生命上限（升级后全体）|
| **分散光** (ScatteredLight_Yuka) | 1→0 | 能力 | `[烈阳]` 最多消耗 4 点阳光，每消耗 1 点获得持续 4 回合的每回合 4 阳光 |
| **吹拂** (Breeze_Yuka) | 1 | 技能 | `[烈阳]` 最多消耗 4/5 点阳光；每消耗 1 点，所有向日葵生命上限提高 1 点 |
| **烈日晴空** (ClearSky_Yuka) | 0 | 技能 | 固定消耗 1 点阳光；之后 1/2 次获得阳光时额外获得 4 点 |
| **花冢** (FlowerMound_Yuka) | 2 | 能力 | 向日葵失去生命时获得 3/4 点阳光 |
| **大妖怪的优雅** (GreatYoukaiElegance_Yuka) | 2 | 技能 | 获得 14/16 点格挡；每朵向日葵额外获得 4/5 点格挡 |

#### 稀有卡（Rare）

| 卡牌 | 费用 | 类型 | 效果概要 |
|------|------|------|----------|
| **无限螺旋** (InfiniteSpiral_Yuka) | 0 | 能力 | 所有向日葵 +6/8 最大生命，抽 2/3 张，获得 2/3 能量。每次出牌向日葵会攻击。回合结束时死亡 |
| **烈阳重发** (SunlightRepeater_Yuka) | 2 | 攻击 | `[烈阳]` 造成 8+4x 伤害（x 为消耗阳光数，上限 8/10）。支持免费卡牌的满额计算 |
| **暴君抹杀** (TyrantObliteration_Yuka) | 2 | 攻击 | `[烈阳]` 造成 8/9 点伤害，初始 1 次，每消耗 1 阳光攻击次数 +1，上限 4/6 |
| **残暴** (Brutality_Yuka) | 3→2 | 攻击 | 所有向日葵失去当前生命值的一半，对所有敌人造成等量伤害 |

---

### 2.3 能力 / Power（共 11 个）

| Power | 类型 | 叠加方式 | 效果 |
|-------|------|----------|------|
| **向阳** (XiangYangPower) | Buff | Counter | 回合开始时获得等同于层数的阳光 |
| **高照** (HighIlluminationPower) | Buff | Counter | 回合结束时对最低血量敌人造成等同于层数的伤害 |
| **守护** (SunflowerGuardPower) | Buff | Single | 向日葵为玩家承受下一次未被格挡的伤害（挡刀机制）|
| **花的报恩** (FlowerGratitudePower) | Buff | Counter | 向日葵回血/加生命上限时，抽等同于层数的卡 |
| **无限螺旋** (InfiniteSpiralPower) | Debuff | Counter | 每次出牌所有向日葵攻击最低血量敌人；回合结束时玩家死亡 |
| **下午茶（随机）** (AfternoonTeaRandomPower) | Buff | Counter | 本回合阳光变化时，随机一朵向日葵 +1 生命上限 |
| **下午茶（全体）** (AfternoonTeaAllPower) | Buff | Counter | 本回合阳光变化时，所有向日葵 +1 生命上限 |
| **分散光** (ScatteredLightPower) | Buff | Counter | 回合开始时获得 4 点阳光，层数 -1 |
| **围绕** (SurroundPower) | Buff | Counter | 每次召唤向日葵时额外提高其最大生命值，并给予守护 |
| **烈日晴空** (ClearSkyPower) | Buff | Counter | 接下来 1/2 次获得阳光时额外获得 4 点阳光 |
| **花冢** (FlowerMoundPower) | Buff | Counter | 向日葵失去生命时获得 3/4 点阳光 |

---

### 2.4 遗物（共 1 个）

| 遗物 | 稀有度 | 效果 |
|------|--------|------|
| **太阳草帽** (KazamiYukaStarterRelic) | Starter | 进入战斗房间时获得 3 点阳光。每召唤一朵向日葵给予其 1 层 `向阳` |

> 这是角色的**初始遗物**，通过 `[RegisterCharacterStarterRelic]` 自动绑定。

---

### 2.5 向日葵系统（核心机制）

#### 向日葵怪物
- `KazamiYukaSunflower` — 继承 `ModMonsterTemplate`
- 初始生命：10/10
- 行为：空（`NOTHING_MOVE`），不会主动行动
- 动画状态：`idle`, `hit`, `attack`, `die`
- 通过 `PlayerCmd.AddPet<KazamiYukaSunflower>` 召唤
- 召唤上限：5 朵
- 召唤后自动放置在玩家与最近存活怪物之间的安全区域，不需要玩家手动选位置
- 向日葵显示层为玩家 `ZIndex + 1`，保留交互以支持向日葵目标卡，但不作为玩家或怪物的碰撞阻挡物

#### SunflowerManager（全局静态管理器）
| 功能 | 说明 |
|------|------|
| `Summon` | 召唤向日葵，设置初始生命，自动附加 `向阳`、播放音效 |
| `GetSunflowers` | 获取玩家当前场上的所有向日葵 |
| `HealSingle / HealAll` | 单体 / 群体治疗，触发 `FlowerHealthChanged` 事件 |
| `GainMaxHpSingle / GainMaxHpAll` | 单体 / 群体增加最大生命，触发事件 |
| `RegisterHealthListener` | 注册向日葵血量变化监听器 |

#### 向日葵挡刀机制
- 通过 `SunflowerGuardPatch` 实现
- 玩家受到伤害时，带有 `守护` 状态的向日葵按血量从低到高依次吸收伤害

---

### 2.6 阳光系统（第二资源）

#### SunlightManager（全局静态管理器）
- 支持**玩家专属阳光池** + **全局计数**
- 每场战斗开始时自动清零
- 提供事件 `SunlightChanged` 供 UI 订阅

| 方法 | 说明 |
|------|------|
| `GainSunlight(player, amount)` | 获得阳光 |
| `TrySpendSunlight(player, amount)` | 尝试消耗阳光（检查玩家专属池）|
| `HasEnoughSunlight(player, amount)` | 检查阳光是否足够 |
| `ResetForCombat()` | 战斗开始时重置 |

#### UI 显示
- `NSunlightCounter` — 阳光计数器 UI，挂载在官方 `EnergyCounterContainer` 下
- 通过 `InjectSunlightCounterPatch` 在战斗开始时自动注入
- 阳光为 0 时显示红色并隐藏计数器

#### 卡牌上的阳光消耗显示
- `CardSunlightCostPatch` — 在卡牌 UI 上叠加小太阳图标和数字
- `SunlightCostVar` — 自定义 DynamicVar，支持实时检测免费状态（药水、临时减费等）
- `SunlightIconsFormatter` — 本地化解析器，将 `{...:sunlightIcons}` 转换为 `[sunlight]` 标签
- `CustomTextIconPatch` — 将 `[sunlight]` 标签渲染为阳光图标图片
- `LocFormatterPatch` — 向 SmartFormat 注入自定义格式化器
- 固定消耗阳光的卡牌必须通过 `SunlightCostVar` + `TryPayFixedSunlight(...)` 挂载真实消耗；卡牌描述不重复书写“消耗阳光”，费用由 `CardSunlightCostPatch` 显示。
- `[烈阳] X` 表示至多消耗 `X` 点阳光；普通固定阳光费用表示至少需要并固定消耗对应数量。

---

### 2.7 Harmony Patch（共 8 个）

| Patch 文件 | 作用 |
|-----------|------|
| `UITargetingVisualsPatch` | 拦截 `NTargetManager.StartTargeting`，为实现了 `ICustomTargetValidator` 的卡牌添加 UI 目标过滤（仅向日葵可选中高亮）|
| `UniversalTargetPatch` | 拦截 `CardModel.IsValidTarget`，在逻辑层再次验证自定义目标（仅向日葵）|
| `SunflowerGuardPatch` | 拦截 `Hook.ModifyHpLostAfterOsty`，实现向日葵挡刀（伤害吸收）|
| `CustomTextIconPatch` | 拦截 `MegaRichTextLabel.SetTextAutoSize`，将 `[sunlight]` 替换为 `img` 标签渲染图标 |
| `LocFormatterPatch` | 拦截 `LocManager.LoadLocFormatters`，注入 `SunlightIconsFormatter` |
| `CardCostFilterPatch` | 拦截 `CardModel.CostsEnergyOrStars`，让有阳光消耗的卡牌能在药水选择界面正确显示 |
| `InjectSunlightCounterPatch` | 拦截 `NCombatUi.Activate`，战斗开始时重置阳光并注入阳光计数器 UI |
| `CardSunlightCostPatch` | 拦截 `NCard._Ready` 和 `UpdateVisuals`，在卡牌上显示阳光消耗图标 |

---

### 2.8 接口与基类

| 文件 | 说明 |
|------|------|
| `KazamiYukaCard` | 卡牌基类，继承 `ModCardTemplate`。核心功能：<br>- `IsSunlightFree` 属性：检测卡牌是否被药水/遗物临时免费<br>- `IsPlayable` 重写：阳光不足时阻止打出<br>- `IsFreeFromReceipt`：通过 `CardPlay` 收据判定是否免单<br>- `TryPayFixedSunlight`：固定阳光消耗接口<br>- `ConsumeUpToMaxSunlight`：最多消耗 N 点阳光接口 |
| `ICustomTargetValidator` | 自定义目标验证接口，`IsValidCustomTarget(Creature)` |
| `IFixedSunlightCard` / `IVariableSunlightCard` | 固定/可变阳光消耗卡牌接口 |
| `SunlightCardExtensions` | 扩展方法：`TryPayFixedSunlight`、`ConsumeVariableSunlight` |

---

### 2.9 自定义节点

| 文件 | 说明 |
|------|------|
| `NSunlightCounter` | 阳光计数器 UI 节点（继承 Godot `Control`）|
| `YukaEnergyCounter` | 幽香能量计数器（继承 `NEnergyCounter`，目前空实现占位）|
| `SunflowerVisuals` | 向日葵视觉节点（继承 `NCreatureVisuals`）|
| `KazamiYukaMerchantCharacter` | 商店角色节点（继承 `NMerchantCharacter`）|
| `KazamiYukaRestsiteCharacter` | 篝火角色节点（继承 `NRestSiteCharacter`）|
| `KazamiYukaSelectionReticle` | 选择指示器（继承 `NSelectionReticle`）|

---

### 2.10 本地化（中文 zhs）

| 文件 | 内容 |
|------|------|
| `cards.json` | 23 张卡牌的标题与描述 |
| `powers.json` | 11 个能力的标题与描述 |
| `relics.json` | 1 个遗物的标题、描述、风味文本 |
| `characters.json` | 角色名称、代词、描述、解锁条件、闲聊等 |
| `card_keywords.json` | `成长`、`烈阳` 两个关键词的标题与描述 |

---

### 2.11 图像资源

#### 卡牌图
- ✅ 打击、防御、播下种子、高照、阳光收集、阳伞招架、无限螺旋、下午茶、阳光打击
- ✅ 花的报恩、花丛护身、阳光照耀、烈阳重发、分散光、暴君抹杀、团结、遮挡
- 无专属卡图的卡牌由 `KazamiYukaCard.ResolveCardPortraitPath()` 自动使用 `Temp_Yuka.png`

#### 能力图标（现有能力按代码配置图标）
- 向阳、高照、守护、花的报恩、无限螺旋、下午茶、分散光；新能力沿用现有临时图标

#### 其他图像
- 角色头像 (`kazamiyuka_icon.png`)
- 角色选择图 (`char_select_kazamiyuka.png`)
- 选择背景 (`kazamiyuka_bg.png`)
- 角色立绘 (`kazamiyuka_character.png`)
- 能量图标 (`energy_kazamiyuka.png` / `energy_kazamiyuka_big.png`)
- 阳光图标 (`sunlight.png`、`sunlight_icon.png`)
- 遗物图标 (`KazamiYukaStarterRelic.png`)
- 向日葵 (`SunFlower.png`)

---

## 三、核心机制流程图（文字版）

```
战斗开始
    │
    ▼
[InjectSunlightCounterPatch] 注入阳光计数器 UI
[SunlightManager.ResetForCombat] 阳光清零
    │
    ▼
打出「播下种子」/「团结」
    │
    ▼
[SunflowerManager.Summon] ──► 召唤向日葵（上限 5）
    │                           附加 1 层「向阳」
    │                           播放音效
    ▼
向日葵回合开始
    │
    ▼
[XiangYangPower] ──► 获得阳光（层数 = 获得量）
    │
    ▼
打出消耗阳光的卡牌（如阳光打击、花丛护身等）
    │
    ▼
[KazamiYukaCard.TryPayFixedSunlight] 或 [ConsumeUpToMaxSunlight]
    │   （自动检测是否被药水/遗物免费）
    ▼
扣除阳光 ──► 执行卡牌效果
    │
    ▼
打出「花的报恩」后
    │
    ▼
向日葵回血/加生命上限 ──► [FlowerGratitudePower.TriggerDraw] 抽卡
    │
    ▼
玩家即将受到伤害
    │
    ▼
[SunflowerGuardPatch] ──► 带有「守护」的向日葵挡刀（吸收伤害）
```

---

## 四、还需要补充的内容

### 🔴 高优先级（建议尽快补齐）

| 内容 | 说明 | 影响 |
|------|------|------|
| **更多遗物** | 目前只有初始遗物「太阳草帽」| 角色深度不足，后期玩法单一 |
| **药水（Potion）** | `KazamiYukaPotionPool` 只有空定义 | 药水池未实际使用 |

### 🟡 中优先级（完善体验）

| 内容 | 说明 |
|------|------|
| **角色音效** | `KazamiYukaCharacter.cs` 中攻击、施法、死亡、选择等音效均为注释状态 |
| **卡牌拖尾 VFX** | `AssetProfile.Vfx.TrailPath` 被注释，缺少卡牌拖动时的拖尾特效 |
| **角色过渡动画** | `CharacterSelectTransitionPath` 被注释 |
| **攻击特效列表** | 目前使用的是原版 Ironclad 的攻击特效，建议制作专属特效 |
| **角色动画延迟** | `AttackAnimDelay` 和 `CastAnimDelay` 均为 0，可能需要根据 Spine 动画调整 |
| **YukaEnergyCounter** | 目前为空实现，如果需要自定义能量 UI 需要补充逻辑 |

### 🟢 低优先级（锦上添花）

| 内容 | 说明 |
|------|------|
| **多人模式资源** | 手指、石头剪刀布等资源被注释 |
| **更多事件对话** | `characters.json` 中的 `banter` 只有两条，可以增加更多 |
| **Potion 设计** | 可以设计 2-3 瓶与阳光/向日葵机制相关的药水 |
| **更多稀有卡** | 目前稀有卡只有 3 张，可以扩充到 4-5 张 |
| **成就 / 解锁条件** | `unlockText` 引用了 `{Prerequisite}`，需要配置具体的解锁角色 |
| **场景节点细节** | `.tscn` 场景文件内的具体 Godot 节点配置（如位置、动画参数）需要手动在编辑器中确认和调整 |

### ⚠️ 代码层面可优化项

| 内容 | 说明 |
|------|------|
| `KazamiYukaStarterRelic` 的向日葵向阳逻辑 | 目前 `Summon` 方法内部直接调用 `PowerCmd.Apply<XiangYangPower>`，但 Relic 描述说的是"每当你召唤"，这个耦合在 `SunflowerManager` 内，建议改为通过事件/钩子让 Relic 自己监听召唤事件 |
| `SunflowerGuardPatch` 的伤害吸收 | 当前用 `CreatureCmd.SetCurrentHp` 直接扣血，没有走正式的 `DamageCmd` 流程，可能和某些钩子不兼容 |
| `InfiniteSpiralPower` 的死亡效果 | 使用 `DoomPower.DoomKill`，回合结束直接死亡，体验较极端，可能需要增加预警或特效 |

---

## 五、文件结构总览

```
KazamiYuka/
├── Scripts/
│   ├── Entry.cs                          # Mod 入口
│   ├── KazamiYukaCharacter.cs            # 角色定义
│   ├── KazamiYukaPools.cs                # 卡池/遗物池/药水池
│   ├── KazamiYukaKeywords.cs             # 关键词注册
│   ├── ICustomTargetValidator.cs         # 自定义目标验证接口
│   ├── SunflowerManager.cs               # 向日葵管理器
│   ├── SunlightManager.cs                # 阳光资源管理器
│   │
│   ├── Cards/                            # 卡牌（19 张）
│   │   ├── KazamiYukaCard.cs             # 卡牌基类
│   │   ├── Strike_Yuka.cs
│   │   ├── Defend_Yuka.cs
│   │   ├── SowSeeds_Yuka.cs
│   │   ├── Sunshine_Yuka.cs
│   │   ├── HighIllumination_Yuka.cs
│   │   ├── SunlightCollection_Yuka.cs
│   │   ├── Photosynthesis_Yuka.cs
│   │   ├── UmbrellaParry_Yuka.cs
│   │   ├── SunlightStrike_Yuka.cs
│   │   ├── FlowerBedProtection_Yuka.cs
│   │   ├── Unity_Yuka.cs
│   │   ├── Shading_Yuka.cs
│   │   ├── FlowerGratitude_Yuka.cs
│   │   ├── AfternoonTea_Yuka.cs
│   │   ├── ScatteredLight_Yuka.cs
│   │   ├── InfiniteSpiral_Yuka.cs
│   │   ├── SunlightRepeater_Yuka.cs
│   │   └── TyrantObliteration_Yuka.cs
│   │
│   ├── Powers/                           # 能力（8 个）
│   │   ├── XiangYangPower.cs
│   │   ├── HighIlluminationPower.cs
│   │   ├── SunflowerGuardPower.cs
│   │   ├── FlowerGratitudePower.cs
│   │   ├── InfiniteSpiralPower.cs
│   │   ├── AfternoonTeaPower.cs
│   │   └── ScatteredLightPower.cs
│   │
│   ├── relics/                           # 遗物（1 个）
│   │   └── KazamiYukaStarterRelic.cs
│   │
│   ├── Patches/                          # Harmony Patch（8 个）
│   │   ├── UITargetingVisualsPatch.cs
│   │   ├── UniversalTargetPatch.cs
│   │   ├── SunflowerGuardPatch.cs
│   │   ├── CustomTextIconPatch.cs
│   │   ├── LocFormatterPatch.cs
│   │   ├── CardCostFilterPatch.cs
│   │   ├── InjectSunlightCounterPatch.cs
│   │   └── CardSunlightCostPatch.cs
│   │
│   ├── Nodes/                            # 自定义 Godot 节点
│   │   ├── NSunlightCounter.cs
│   │   ├── YukaEnergyCounter.cs
│   │   ├── SunflowerVisuals.cs
│   │   ├── KazamiYukaMerchantCharacter.cs
│   │   ├── KazamiYukaRestsiteCharacter.cs
│   │   └── KazamiYukaSelectionReticle.cs
│   │
│   ├── SunFlower/                        # 向日葵相关
│   │   ├── KazamiYukaSunflower.cs        # 向日葵怪物模板
│   │   ├── SunflowerManager.cs           #（注意：实际在 Scripts/ 根目录下）
│   │   └── SunlightManager.cs            #（注意：实际在 Scripts/ 根目录下）
│   │
│   ├── Vars/                             # 自定义动态变量
│   │   └── SunlightCostVar.cs
│   │
│   ├── Formatters/                       # 本地化格式化器
│   │   └── SunlightIconsFormatter.cs
│   │
│   └── Interfaces/                       # 阳光卡牌接口
│       └── SunlightCardInterfaces.cs
│
├── mods/KazamiYuka/
│   ├── scenes/                           # .tscn 场景文件（9 个）
│   └── images/                           # 图像资源
│       ├── cards/                        # 卡牌图（18 张）
│       ├── powers/                       # 能力图标（8 组）
│       ├── relics/                       # 遗物图标（1 个）
│       └── ...                           # 其他角色/能量/阳光图标
│
├── KazamiYuka/localization/zhs/          # 中文本地化
│   ├── cards.json
│   ├── powers.json
│   ├── relics.json
│   ├── characters.json
│   └── card_keywords.json
│
├── lib/
│   └── 0Harmony.dll                      # Harmony 依赖
│
└── KazamiYuka.json                       # Mod 元数据
```

---

## 六、开发统计

| 类别 | 数量 |
|------|------|
| 卡牌 | 19 张 |
| 能力 | 8 个 |
| 遗物 | 1 个 |
| Harmony Patch | 8 个 |
| 自定义关键词 | 2 个 |
| 自定义接口 | 3 个 |
| 自定义节点 | 6 个 |
| C# 脚本文件 | ~35 个 |
| 场景文件 (.tscn) | 9 个 |
| 本地化文件 | 5 个 |

---

*文档由项目代码自动生成，如有更新请同步修改此文档。*
