using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Data.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using MegaCrit.Sts2.Core.Nodes; 
using STS2RitsuLib.Utils;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Scaffolding.Godot; 

namespace KazamiYuka.Scripts;

[RegisterCharacter]
public class KazamiYukaCharacter : ModCharacterTemplate<KazamiYukaCardPool, KazamiYukaRelicPool, KazamiYukaPotionPool>
{
    public override Color NameColor => new(0f, 0.8f, 0f);
    public override Color DreadNameColor => new("00CC00C0");
    public override Color EnergyLabelOutlineColor => new(0.654f, 0.333f, 0.345f);
    public override Color MapDrawingColor => new(0f, 0.8f, 0f);

    public override CharacterGender Gender => CharacterGender.Feminine;

    public override int StartingHp => 84;
    public override int StartingGold => 99;

    public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles.Merge(
        CharacterAssetProfiles.Ironclad(),
        new(
            Scenes: new(
                VisualsPath: "res://mods/KazamiYuka/scenes/kazamiyuka_character.tscn",
                EnergyCounterPath: "res://mods/KazamiYuka/scenes/kazamiyuka_energy_counter.tscn",
                MerchantAnimPath: "res://mods/KazamiYuka/scenes/kazamiyuka_character_merchant.tscn",
                RestSiteAnimPath: "res://mods/KazamiYuka/scenes/kazamiyuka_character_rest_site.tscn"
            ),
            Ui: new(
                IconTexturePath: "res://mods/KazamiYuka/images/kazamiyuka_icon.png",
                IconPath: "res://mods/KazamiYuka/scenes/icon.tscn",
                CharacterSelectBgPath: "res://mods/KazamiYuka/scenes/kazamiyuka_bg.tscn",
                CharacterSelectIconPath: "res://mods/KazamiYuka/images/char_select_kazamiyuka.png",
                CharacterSelectLockedIconPath: "res://mods/KazamiYuka/images/char_select_kazamiyuka.png",
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
                CharacterSelectSfx: "event:/sfx/characters/kazamiyuka/select"
                // 过渡音效
                // CharacterTransitionSfx: "event:/sfx/ui/wipe_ironclad"
            ),
            Multiplayer: new(
                // 多人模式-手指。
                ArmPointingTexturePath: "res://mods/KazamiYuka/images/hands/zhi.png",
                // 多人模式剪刀石头布-石头。
                ArmRockTexturePath: "res://mods/KazamiYuka/images/hands/shitou.png",
                // 多人模式剪刀石头布-布。
                ArmPaperTexturePath: "res://mods/KazamiYuka/images/hands/bu.png",
                // 多人模式剪刀石头布-剪刀。
                ArmScissorsTexturePath: "res://mods/KazamiYuka/images/hands/jiandao.png"
            )));

    // 攻击和施法动画延迟，以对齐动画
    public override float AttackAnimDelay => 0f;
    public override float CastAnimDelay => 0f;

    public override CreatureAnimator GenerateAnimator(MegaSprite controller, Creature creature)
    {
        var idle = new AnimState("idle_loop", true);
        var lowHealth = new AnimState("low_health_loop", true);
        var animator = new CreatureAnimator(IsLowHealth(creature) ? lowHealth : idle, controller);
        animator.AddAnyState("Idle", idle, () => !IsLowHealth(creature));
        animator.AddAnyState("Idle", lowHealth, () => IsLowHealth(creature));
        animator.AddAnyState("Relaxed", idle);
        animator.AddAnyState("Dead", new AnimState("die", false));

        foreach (var (state, trigger) in AnimationStates)
        {
            state.AddNextState(idle, () => !IsLowHealth(creature));
            state.AddNextState(lowHealth, () => IsLowHealth(creature));
            animator.AddAnyState(trigger, state);
        }

        return animator;
    }

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
