using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.DynamicVars;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class Manjusaka_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new SunlightCostVar(3m)
    ];

    public override CardMultiplayerConstraint MultiplayerConstraint
        => CardMultiplayerConstraint.MultiplayerOnly;

    public Manjusaka_Yuka()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!TryPayFixedSunlight(base.Owner, DynamicVars["SunlightCost"].IntValue, cardPlay))
        {
            return;
        }

        await PowerCmd.Apply<ManjusakaPower>(
            choiceContext,
            base.Owner.Creature,
            1m,
            base.Owner.Creature,
            this,
            false
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SunlightCost"].UpgradeValueBy(-1m);
        EnergyCost.UpgradeBy(-1);
    }
}
