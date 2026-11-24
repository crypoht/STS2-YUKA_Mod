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
using STS2RitsuLib.CardTags;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.Powers; 
using KazamiYuka.Scripts.DynamicVars;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]

public class FlowerBedProtection_Yuka : KazamiYukaCard, ICustomTargetValidator
{
    protected override HashSet<CardTag> CanonicalTags => [SunflowerCardTags.Sunflower];

    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new SunlightCostVar(4m),
        new DynamicVar("MaxHpGain", 4m)
    ];

    public FlowerBedProtection_Yuka() : base(energyCost, type, rarity, KazamiYukaTargetTypes.Sunflower, shouldShowInCardLibrary)
    {
    }

    public bool IsValidCustomTarget(Creature target)
    {
        return target.Monster is KazamiYukaSunflower;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        var target = cardPlay.Target;

        if (player == null || target == null) return;

        if (target.Monster is KazamiYukaSunflower specificFlower)
        {
            int requiredSunlight = (int)DynamicVars["SunlightCost"].BaseValue;
            if (base.TryPayFixedSunlight(player, requiredSunlight, cardPlay))
            {
                decimal hpGain = DynamicVars["MaxHpGain"].BaseValue;

                await SunflowerManager.GainMaxHpSingle(choiceContext, specificFlower.Creature, hpGain);
                
                VfxCmd.PlayOnCreatureCenter(specificFlower.Creature, "vfx/vfx_heal_osty");

                await PowerCmd.Apply<SunflowerGuardPower>(choiceContext, specificFlower.Creature, 1m, player.Creature, this, false);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxHpGain"].UpgradeValueBy(2m);
    }
}
