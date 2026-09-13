using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
using KazamiYuka.Scripts.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class ClearSky_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new SunlightCostVar(1m),
        new DynamicVar("TriggerCount", 1m),
        new DynamicVar("BonusSunlight", 4m)
    ];

    public ClearSky_Yuka() : base(0, CardType.Skill, CardRarity.Common, TargetType.None, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner;
        if (player == null) return;

        int sunlightCost = (int)DynamicVars["SunlightCost"].BaseValue;
        if (!TryPayFixedSunlight(player, sunlightCost, cardPlay)) return;

        await PowerCmd.Apply<ClearSkyPower>(
            choiceContext,
            player.Creature,
            DynamicVars["TriggerCount"].BaseValue,
            player.Creature,
            this,
            false
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars["TriggerCount"].UpgradeValueBy(1m);
    }
}
