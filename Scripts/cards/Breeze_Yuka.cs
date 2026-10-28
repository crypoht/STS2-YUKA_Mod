using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.DynamicVars;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class Breeze_Yuka : KazamiYukaCard
{
    protected override HashSet<CardTag> CanonicalTags => [SunflowerCardTags.Sunflower];

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new SunlightCostVar(4m),
        new CalculationBaseVar(1m),
        new CalculationExtraVar(0m),
        new CalculatedVar("Energy").WithMultiplier((CardModel card, Creature? _) =>
            card.Owner == null
                ? 0m
                : SunflowerManager.GetSunflowers(card.Owner).Count(flower => flower.IsAlive)),
        new CalculatedVar("Cards").WithMultiplier((CardModel card, Creature? _) =>
            card.Owner == null
                ? 0m
                : SunflowerManager.GetSunflowers(card.Owner).Count(flower => flower.IsAlive))
    ];

    public Breeze_Yuka() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner;
        if (player == null || !TryPayFixedSunlight(player, DynamicVars["SunlightCost"].IntValue, cardPlay))
        {
            return;
        }

        int sunflowerCount = SunflowerManager.GetSunflowers(player).Count(flower => flower.IsAlive);
        await CardPileCmd.Draw(choiceContext, sunflowerCount, player, false);
        await PlayerCmd.GainEnergy(DynamicVars["Energy"].IntValue * sunflowerCount, player);
    }

    protected override void OnUpgrade() => base.EnergyCost.UpgradeBy(-1);
}
