using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class FlowerMound_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("SunlightGain", FlowerMoundPower.SunlightPerStack),
        new EnergyVar(FlowerMoundPower.EnergyPerStack)
    ];

    public FlowerMound_Yuka() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner;
        if (player == null) return;

        await PowerCmd.Apply<FlowerMoundPower>(
            choiceContext,
            player.Creature,
            1m,
            player.Creature,
            this,
            false
        );
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
