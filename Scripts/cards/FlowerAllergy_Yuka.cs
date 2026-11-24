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
public sealed class FlowerAllergy_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new PowerVar<MegaCrit.Sts2.Core.Models.Powers.WeakPower>(1m),
        new PowerVar<ParasitePower>(1m),
        new SunlightCostVar(2m),
    ];

    protected override IEnumerable<MegaCrit.Sts2.Core.HoverTips.IHoverTip> AdditionalHoverTips => [
        MegaCrit.Sts2.Core.HoverTips.HoverTipFactory.FromPower<MegaCrit.Sts2.Core.Models.Powers.WeakPower>(null),
        MegaCrit.Sts2.Core.HoverTips.HoverTipFactory.FromPower<ParasitePower>(null)
    ];

    public FlowerAllergy_Yuka()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!TryPayFixedSunlight(base.Owner, DynamicVars["SunlightCost"].IntValue, cardPlay))
        {
            return;
        }

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, base.Owner, false);

        if (base.CombatState?.HittableEnemies is not { } enemies)
        {
            return;
        }

        await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.WeakPower>(
            choiceContext,
            enemies,
            DynamicVars["WeakPower"].BaseValue,
            base.Owner.Creature,
            this,
            false
        );
        await PowerCmd.Apply<ParasitePower>(
            choiceContext,
            enemies,
            DynamicVars["ParasitePower"].BaseValue,
            base.Owner.Creature,
            this,
            false
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
        DynamicVars["ParasitePower"].UpgradeValueBy(1m);
    }
}
