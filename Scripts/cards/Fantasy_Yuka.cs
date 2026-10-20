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
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
[RegisterDustyTomeCard(typeof(KazamiYukaCharacter))]
public sealed class Fantasy_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxHpGain", 1m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [KazamiYukaKeywords.FierceSunInfinite];

    public Fantasy_Yuka() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AllEnemies, true) { }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        int consumedSunlight = IsFreeFromReceipt(play)
            ? SunlightManager.GetSunlight(base.Owner)
            : SunlightManager.SpendAllSunlight(base.Owner);
        if (consumedSunlight <= 0)
        {
            return;
        }

        await SunflowerManager.GainMaxHpAll(
            context,
            base.Owner,
            consumedSunlight * DynamicVars["MaxHpGain"].BaseValue
        );

        foreach (Creature flower in SunflowerManager.GetSunflowers(base.Owner).Where(flower => flower.IsAlive).ToList())
        {
            Creature? target = GetLowestAliveEnemy();
            if (target == null)
            {
                break;
            }

            await CreatureCmd.Damage(context, target, flower.MaxHp, ValueProp.Move, flower, this, play);
        }
    }

    protected override void OnUpgrade() => base.EnergyCost.UpgradeBy(-1);
}
