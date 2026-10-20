using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.DynamicVars;
using KazamiYuka.Scripts.Keywords;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class SunlightDiffusion_Yuka : KazamiYukaCard
{
    protected override HashSet<CardTag> CanonicalTags => [SunflowerCardTags.Sunflower];

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new SunlightCostVar(2m),
        new PowerVar<XiangYangPower>(1m),
        new DynamicVar("TargetCount", 2m)
    ];

    public SunlightDiffusion_Yuka()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (!TryPayFixedSunlight(base.Owner, DynamicVars["SunlightCost"].IntValue, play))
        {
            return;
        }

        List<Creature> sunflowers = SunflowerManager.GetSunflowers(base.Owner)
            .Where(flower => flower.IsAlive)
            .ToList();
        if (sunflowers.Count == 0)
        {
            return;
        }

        for (int index = 0; index < DynamicVars["TargetCount"].IntValue; index++)
        {
            Creature sunflower = sunflowers[
                base.Owner.RunState.Rng.CombatCardGeneration.NextInt(sunflowers.Count)];
            await PowerCmd.Apply<XiangYangPower>(
                context,
                sunflower,
                DynamicVars["XiangYangPower"].BaseValue,
                base.Owner.Creature,
                this,
                false
            );
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["TargetCount"].UpgradeValueBy(1m);
    }
}
