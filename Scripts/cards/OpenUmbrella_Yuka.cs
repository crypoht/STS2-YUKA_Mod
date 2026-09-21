using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
using KazamiYuka.Scripts.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class OpenUmbrella_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new SunlightCostVar(1m)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public OpenUmbrella_Yuka() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (TryPayFixedSunlight(base.Owner, DynamicVars["SunlightCost"].IntValue, play))
            await PowerCmd.Apply<OpenUmbrellaPower>(context, base.Owner.Creature, 1m, base.Owner.Creature, this, false);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
