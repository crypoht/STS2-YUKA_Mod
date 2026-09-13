using Godot;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Data.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using MegaCrit.Sts2.Core.Nodes; // 🌟 修复之前报错的 NCreatureVisuals 引用
using STS2RitsuLib.Utils;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Scaffolding.Godot; // 🌟 修复之前报错的 RitsuGodotNodeFactories 引用

namespace KazamiYuka.Scripts;

[RegisterCharacter]
public class KazamiYukaCharacter : ModCharacterTemplate<KazamiYukaCardPool, KazamiYukaRelicPool, KazamiYukaPotionPool>
{
    // 角色名称颜色 (#00cc00 纯绿)
    public override Color NameColor => new(0f, 0.8f, 0f);
    // 能量图标轮廓颜色
    public override Color EnergyLabelOutlineColor => new(0.654f, 0.333f, 0.345f);
    // 地图绘制颜色
    public override Color MapDrawingColor => new(0f, 0.8f, 0f);

    // 人物性别（幽香是女性）
    public override CharacterGender Gender => CharacterGender.Feminine;

    // 初始血量和金币
    public override int StartingHp => 84;
    public override int StartingGold => 99;

    public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles.Merge(
        CharacterAssetProfiles.Ironclad(),
        new(
            Scenes: new(
                // 人物模型tscn路径。
                VisualsPath: "res://mods/KazamiYuka/scenes/kazamiyuka_character.tscn",
                // 能量表盘tscn路径。
                EnergyCounterPath: "res://mods/KazamiYuka/scenes/kazamiyuka_energy_counter.tscn",
                // 商店人物场景。
                MerchantAnimPath: "res://mods/KazamiYuka/scenes/kazamiyuka_character_merchant.tscn",
                // 篝火休息场景。
                 RestSiteAnimPath: "res://mods/KazamiYuka/scenes/kazamiyuka_character_rest_site.tscn"
            ),
            Ui: new(
                // 人物头像路径。
                IconTexturePath: "res://mods/KazamiYuka/images/kazamiyuka_icon.png",
                // 人物头像2号。
                IconPath: "res://mods/KazamiYuka/scenes/icon.tscn",
                // 人物选择背景。
                CharacterSelectBgPath: "res://mods/KazamiYuka/scenes/kazamiyuka_bg.tscn",
                // 人物选择图标。
                CharacterSelectIconPath: "res://mods/KazamiYuka/images/char_select_kazamiyuka.png",
                // 人物选择图标-锁定状态。
                CharacterSelectLockedIconPath: "res://mods/KazamiYuka/images/char_select_kazamiyuka.png",
                // 人物选择过渡动画。
                // CharacterSelectTransitionPath: "res://materials/transitions/ironclad_transition_mat.tres",
                // 地图上的角色标记图标、表情轮盘上的角色头像
                MapMarkerPath:"res://mods/KazamiYuka/images/kazamiyuka_icon.png"
            ),
            Vfx: new(
                // 卡牌拖尾场景。
                // TrailPath: "res://scenes/vfx/card_trail_ironclad.tscn"
            ),
            Audio: new(
                // 攻击音效
                // AttackSfx: null,
                // 施法音效
                // CastSfx: null,
                // 死亡音效
                // DeathSfx: null,
                // 角色选择音效
                // CharacterSelectSfx: null,
                // 过渡音效
                // CharacterTransitionSfx: "event:/sfx/ui/wipe_ironclad"
            ),
            Multiplayer: new(
                // 多人模式-手指。
                // ArmPointingTexturePath: null,
                // 多人模式剪刀石头布-石头。
                // ArmRockTexturePath: null,
                // 多人模式剪刀石头布-布。
                // ArmPaperTexturePath: null,
                // 多人模式剪刀石头布-剪刀。
                // ArmScissorsTexturePath: null
            )));

    // 攻击和施法动画延迟，以对齐动画
    public override float AttackAnimDelay => 0f;
    public override float CastAnimDelay => 0f;

    // 如果你的人物不需要时间线小故事，加上这句。
    // public override bool RequiresEpochAndTimeline => false;

    // 自动转换人物场景，让你不需要手动挂脚本。复制即可。
    protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.Scenes!.VisualsPath!);

    // 初始卡组，或者在卡牌类上用RegisterCharacterStarterCard就不用写这个
    // protected override IEnumerable<StartingDeckEntry> StartingDeckEntries => [
    //     new(typeof(TestCard), 5)
    // ];

    // 初始遗物，或者在遗物类上用RegisterCharacterStarterRelic就不用写这个
    // protected override IEnumerable<Type> StartingRelicTypes => [
    //     typeof(Akabeko)
    // ];

    // 攻击建筑师的攻击特效列表
    public override List<string> GetArchitectAttackVfx() => [
        "vfx/vfx_attack_blunt",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact",
        "vfx/vfx_rock_shatter"
    ];
}