using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class FlowerGardenDance_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new SummonVar(5),
        SunflowerDamageVar(7m)
    ];

    public FlowerGardenDance_Yuka()
        : base(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KazamiYukaKeywords.Grow
    ];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        await SunflowerManager.Summon(
            base.Owner,
            DynamicVars["Summon"].BaseValue
        );

        foreach (Creature sunflower in SunflowerManager.GetSunflowers(base.Owner).Where(flower => flower.IsAlive))
        {
            Creature? target = GetLowestAliveEnemy();
            if (target == null)
            {
                break;
            }

            await CreatureCmd.Damage(
                context,
                target,
                DynamicVars["SunflowerDamage"].BaseValue,
                ValueProp.Move,
                sunflower,
                this,
                play
            );
        }
    }

    protected override void OnUpgrade() => DynamicVars["SunflowerDamage"].UpgradeValueBy(3m);
}
