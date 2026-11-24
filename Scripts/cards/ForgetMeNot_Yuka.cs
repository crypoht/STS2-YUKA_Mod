using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class ForgetMeNot_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxHpGain", 2m)
    ];

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Eternal];

    public ForgetMeNot_Yuka()
        : base(0, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        if (card == this)
        {
            await CardCmd.AutoPlay(
                choiceContext,
                this,
                null,
                AutoPlayType.Default,
                false,
                false);
        }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (IsUpgraded)
        {
            await PowerCmd.Apply<ForgetMeNotUpgradedPower>(
                choiceContext,
                base.Owner.Creature,
                1m,
                base.Owner.Creature,
                this,
                false);
            return;
        }

        await PowerCmd.Apply<ForgetMeNotPower>(
            choiceContext,
            base.Owner.Creature,
            1m,
            base.Owner.Creature,
            this,
            false);
    }

    protected override void OnUpgrade()
    {
    }

}
