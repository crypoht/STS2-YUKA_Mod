using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
public sealed class MakeItStruggle_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(21m, ValueProp.Move),
        new DynamicVar("SunflowerDamage", 4m)
    ];

    public MakeItStruggle_Yuka()
        : base(3, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Target == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .Execute(context);

        if (!play.Target.IsAlive)
        {
            return;
        }

        foreach (Creature sunflower in SunflowerManager.GetSunflowers(base.Owner).Where(flower => flower.IsAlive))
        {
            await CreatureCmd.Damage(
                context,
                play.Target,
                DynamicVars["SunflowerDamage"].BaseValue,
                ValueProp.Unpowered | ValueProp.Move,
                sunflower,
                this,
                play
            );
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["SunflowerDamage"].UpgradeValueBy(2m);
    }
}
