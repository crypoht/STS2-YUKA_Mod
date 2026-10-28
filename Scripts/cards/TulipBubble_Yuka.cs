using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts.Powers;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class TulipBubble_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("SunlightGain", 12m),
        new DynamicVar("SunlightLoss", 2m)
    ];

    public TulipBubble_Yuka()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        SunlightManager.GainSunlight(
            base.Owner,
            DynamicVars["SunlightGain"].IntValue);
        await PowerCmd.Apply<BubblePower>(
            choiceContext,
            base.Owner.Creature,
            DynamicVars["SunlightLoss"].BaseValue,
            base.Owner.Creature,
            this,
            false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SunlightGain"].UpgradeValueBy(4m);
    }
}
