using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using KazamiYuka.Scripts.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
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
        new BlockVar(7m, ValueProp.Move),
        new DynamicVar("SunlightPerBlock", 4m),
        new DynamicVar("BlockPerThreshold", 1m),
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

        int sunlightBeforePayment = SunlightManager.GetSunlight(player);
        if (!TryPayFixedSunlight(player, DynamicVars["SunlightCost"].IntValue, cardPlay)) return;

        decimal block = DynamicVars.Block.BaseValue
            + Math.Floor(sunlightBeforePayment / DynamicVars["SunlightPerBlock"].BaseValue)
            * DynamicVars["BlockPerThreshold"].BaseValue;
        await CreatureCmd.GainBlock(player.Creature, block, DynamicVars.Block.Props, cardPlay, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }

}
