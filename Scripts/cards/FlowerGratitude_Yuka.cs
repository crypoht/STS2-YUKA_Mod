using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts.Powers;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.DynamicVars;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class FlowerGratitude_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DrawAmount", 1m),
        new DynamicVar("TriggerCount", 4m),
        new SunlightCostVar(4m),
    ];

    public FlowerGratitude_Yuka() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        if (player == null) return;

        int requiredSunlight = (int)DynamicVars["SunlightCost"].BaseValue;
        if (base.TryPayFixedSunlight(player, requiredSunlight, cardPlay))
        {
            await PowerCmd.Apply<FlowerGratitudePower>(
                choiceContext,
                base.Owner.Creature,
                DynamicVars["TriggerCount"].BaseValue,
                base.Owner.Creature,
                this,
                false
            );
        }
        else
        {
        }
    }

    protected override void OnUpgrade() => base.EnergyCost.UpgradeBy(-1);
}
