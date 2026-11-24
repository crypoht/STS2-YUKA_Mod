using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class VioletsLetter_Yuka : KazamiYukaCard
{
    protected override HashSet<CardTag> CanonicalTags => [SunflowerCardTags.Sunflower];

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust, CardKeyword.Ethereal];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<XiangYangPower>(null),
        HoverTipFactory.FromPower<AutumnBloomPower>(null)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("SunlightMultiplier", 2m),
        new PowerVar<XiangYangPower>(1m)
    ];

    public VioletsLetter_Yuka()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var sunflower in SunflowerManager.GetSunflowers(base.Owner).ToList())
        {
            if (!sunflower.IsAlive)
            {
                continue;
            }

            int currentSunlight = sunflower.GetPower<XiangYangPower>()?.Amount ?? 0;
            int autumnSunlight = sunflower.GetPower<AutumnBloomPower>()?.Amount ?? 0;
            decimal additionalMultiplier = DynamicVars["SunlightMultiplier"].BaseValue - 1m;
            decimal addedSunlight = currentSunlight > 0
                ? currentSunlight * additionalMultiplier
                : autumnSunlight > 0 ? 0m : DynamicVars["XiangYangPower"].BaseValue;

            if (addedSunlight > 0m)
            {
                await PowerCmd.Apply<XiangYangPower>(
                    choiceContext,
                    sunflower,
                    addedSunlight,
                    base.Owner.Creature,
                    this,
                    false);
            }

            if (autumnSunlight > 0)
            {
                await PowerCmd.Apply<AutumnBloomPower>(
                    choiceContext,
                    sunflower,
                    autumnSunlight * additionalMultiplier,
                    base.Owner.Creature,
                    this,
                    false);
            }
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}
