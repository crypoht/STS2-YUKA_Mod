using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.DynamicVars;
using KazamiYuka.Scripts.Keywords;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class FourSeasonsBloom_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new SunlightCostVar(2m),
        new PowerVar<FourSeasonsBloomPower>(1m),
        new DynamicVar("SpringAmount", 2m),
        new DynamicVar("SummerAmount", 4m),
        new DynamicVar("AutumnAmount", 1m),
        new DynamicVar("WinterAmount", 4m)
    ];

    public FourSeasonsBloom_Yuka()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (!TryPayFixedSunlight(
                base.Owner,
                DynamicVars["SunlightCost"].IntValue,
                cardPlay))
        {
            return;
        }

        FourSeasonsBloomPower? power = await PowerCmd.Apply<FourSeasonsBloomPower>(
            choiceContext,
            base.Owner.Creature,
            DynamicVars["FourSeasonsBloomPower"].BaseValue,
            base.Owner.Creature,
            this,
            false
        );
        if (power == null)
        {
            return;
        }

        power.DynamicVars["SpringAmount"].BaseValue = DynamicVars["SpringAmount"].BaseValue;
        power.DynamicVars["SummerAmount"].BaseValue = DynamicVars["SummerAmount"].BaseValue;
        power.DynamicVars["AutumnAmount"].BaseValue = DynamicVars["AutumnAmount"].BaseValue;
        power.DynamicVars["WinterAmount"].BaseValue = DynamicVars["WinterAmount"].BaseValue;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SpringAmount"].UpgradeValueBy(1m);
        DynamicVars["SummerAmount"].UpgradeValueBy(2m);
        DynamicVars["AutumnAmount"].UpgradeValueBy(1m);
        DynamicVars["WinterAmount"].UpgradeValueBy(2m);
    }
}
