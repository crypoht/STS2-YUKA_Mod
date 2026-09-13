using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.Powers; 
using KazamiYuka.Scripts.DynamicVars;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
// 👇 加上你的核心接口：ICustomTargetValidator
public class FlowerBedProtection_Yuka : KazamiYukaCard, ICustomTargetValidator
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new SunlightCostVar(1m),
        new DynamicVar("MaxHpGain", 4m)
    ];

    // 👇 使用 RitsuLib 的 CustomTargetType.Anyone 作为基础，把具体过滤交给你的接口
    public FlowerBedProtection_Yuka() : base(energyCost, type, rarity, KazamiYukaTargetTypes.Sunflower, shouldShowInCardLibrary)
    {
    }

    // 👇 实现你的接口逻辑：只有向日葵才能被选中！
    public bool IsValidCustomTarget(Creature target)
    {
        return target.Monster is KazamiYukaSunflower;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        var target = cardPlay.Target;

        if (player == null || target == null) return;

        // 由于你的 Patch 已经在 UI 和逻辑层做了双重过滤，这里可以直接放心判定它是向日葵
        if (target.Monster is KazamiYukaSunflower specificFlower)
        {
            // 👇 核心修改：读取动态变量，并把 cardPlay 收据交给基类查验！
            int requiredSunlight = (int)DynamicVars["SunlightCost"].BaseValue;
            if (base.TryPayFixedSunlight(player, requiredSunlight, cardPlay))
            {
                decimal hpGain = DynamicVars["MaxHpGain"].BaseValue;

                // 增加最大生命值
                await SunflowerManager.GainMaxHpSingle(choiceContext, specificFlower.Creature, hpGain);
                
                // 播放特效
                VfxCmd.PlayOnCreatureCenter(specificFlower.Creature, "vfx/vfx_heal_osty");

                // 挂上护身状态
                await PowerCmd.Apply<SunflowerGuardPower>(choiceContext, specificFlower.Creature, 1m, player.Creature, this, false);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxHpGain"].UpgradeValueBy(2m);
    }
}
