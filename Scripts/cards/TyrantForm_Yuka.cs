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
public class TyrantForm_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxHpLoss", 4m),
        new DynamicVar("Multiplier", 1m)
    ];

    public TyrantForm_Yuka() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        TyrantFormPower? existingPower = base.Owner.Creature.GetPower<TyrantFormPower>();
        decimal previousStacks = existingPower?.Amount ?? 0m;

        await PowerCmd.Apply<TyrantFormPower>(
            choiceContext,
            base.Owner.Creature,
            DynamicVars["Multiplier"].BaseValue,
            base.Owner.Creature,
            this,
            false
        );

        base.Owner.Creature.GetPower<TyrantFormPower>()?.SetTriggerAmountForCurrentCard(previousStacks);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Multiplier"].UpgradeValueBy(1m);
    }
}
