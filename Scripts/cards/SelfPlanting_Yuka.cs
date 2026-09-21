using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.DynamicVars;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class SelfPlanting_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new SummonVar(27m), new SunlightCostVar(4m)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, KazamiYukaKeywords.Grow];
    public SelfPlanting_Yuka() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true) { }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (TryPayFixedSunlight(base.Owner, DynamicVars["SunlightCost"].IntValue, play))
            await SunflowerManager.Summon(base.Owner, DynamicVars.Summon.BaseValue);
    }
    protected override void OnUpgrade() => DynamicVars.Summon.UpgradeValueBy(9m);
}
