using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class FlowerNameSong_Yuka : KazamiYukaCard
{
    protected override HashSet<CardTag> CanonicalTags => [SunflowerCardTags.Sunflower];

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        SunflowerDamageVar(5m),
        new SunlightCostVar(2m)
    ];

    public FlowerNameSong_Yuka()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!TryPayFixedSunlight(
                base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player,
                DynamicVars["SunlightCost"].IntValue,
                cardPlay))
        {
            return;
        }

        IEnumerable<Creature> enemies = base.CombatState?.HittableEnemies
            .Where(enemy => enemy.IsAlive)
            .ToList() ?? Enumerable.Empty<Creature>();

        foreach (Creature sunflower in SunflowerManager.GetSunflowers(base.Owner)
            .Where(flower => flower.IsAlive))
        {
            if (!enemies.Any())
            {
                break;
            }

            int buffCount = sunflower.Powers.Count(power => power.Type == PowerType.Buff);
            decimal damage = DynamicVars["SunflowerDamage"].BaseValue * (decimal)Math.Pow(2, buffCount);
            await CreatureCmd.Damage(
                choiceContext,
                enemies,
                damage,
                ValueProp.Unpowered | ValueProp.Move,
                sunflower,
                this,
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SunflowerDamage"].UpgradeValueBy(2m);
    }
}
