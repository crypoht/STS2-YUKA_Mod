using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using KazamiYuka.Scripts.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class ClearSky_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(7m),
        new CalculationExtraVar(1m),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) =>
            card.Owner == null
                ? 0m
                : Math.Floor(SunlightManager.GetSunlight(card.Owner) / 3m)),
        new DynamicVar("SunlightPerBlock", 3m),
        new SunlightCostVar(2m)
    ];

    public override bool GainsBlock => true;

    public ClearSky_Yuka() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner;
        if (player == null) return;

        decimal block = DynamicVars.CalculatedBlock.Calculate(cardPlay.Target);
        if (!TryPayFixedSunlight(player, DynamicVars["SunlightCost"].IntValue, cardPlay)) return;

        await CreatureCmd.GainBlock(
            player.Creature,
            block,
            DynamicVars.CalculatedBlock.Props,
            cardPlay,
            false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(3m);
    }

}
