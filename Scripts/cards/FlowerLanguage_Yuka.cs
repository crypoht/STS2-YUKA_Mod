using System.Collections.Generic;
using System.Linq;
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
public sealed class FlowerLanguage_Yuka : KazamiYukaCard
{
    protected override HashSet<CardTag> CanonicalTags => [SunflowerCardTags.Sunflower];

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new SunlightCostVar(1m),
        new PowerVar<FlowerLanguagePower>(3m)
    ];

    public FlowerLanguage_Yuka()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!TryPayFixedSunlight(base.Owner, DynamicVars["SunlightCost"].IntValue, cardPlay))
        {
            return;
        }

        foreach (var sunflower in SunflowerManager.GetSunflowers(base.Owner).Where(flower => flower.IsAlive))
        {
            await PowerCmd.Apply<FlowerLanguagePower>(
                choiceContext,
                sunflower,
                DynamicVars["FlowerLanguagePower"].BaseValue,
                base.Owner.Creature,
                this,
                false
            );
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FlowerLanguagePower"].UpgradeValueBy(1m);
    }
}
