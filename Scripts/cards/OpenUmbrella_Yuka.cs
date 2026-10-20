using System.Collections.Generic;
using System.Linq;
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
    protected override HashSet<CardTag> CanonicalTags => [SunflowerCardTags.Sunflower];

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new SunlightCostVar(4m),
        new PowerVar<OpenUmbrellaPower>(2m)
    ];
    public OpenUmbrella_Yuka() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (!TryPayFixedSunlight(base.Owner, DynamicVars["SunlightCost"].IntValue, play))
        {
            return;
        }

        foreach (var sunflower in SunflowerManager.GetSunflowers(base.Owner).Where(flower => flower.IsAlive))
        {
            await PowerCmd.Apply<OpenUmbrellaPower>(
                context,
                sunflower,
                DynamicVars["OpenUmbrellaPower"].BaseValue,
                base.Owner.Creature,
                this,
                false
            );
        }
    }

    protected override void OnUpgrade() => DynamicVars["OpenUmbrellaPower"].UpgradeValueBy(1m);
}
