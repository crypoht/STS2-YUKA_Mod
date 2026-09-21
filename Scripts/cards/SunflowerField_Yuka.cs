using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Keywords;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class SunflowerField_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Summon", 8m),
        new DynamicVar("Heal", 3m)
    ];

    public SunflowerField_Yuka()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, KazamiYukaKeywords.Grow];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var before = SunflowerManager.GetSunflowers(base.Owner).ToHashSet();
        if (before.Count >= 5)
        {
            await SunflowerManager.Summon(base.Owner, DynamicVars["Summon"].BaseValue);
        }
        else
        {
            while (SunflowerManager.GetSunflowers(base.Owner).Count < 5)
            {
                await SunflowerManager.Summon(base.Owner, DynamicVars["Summon"].BaseValue);
            }
        }

        foreach (var sunflower in SunflowerManager.GetSunflowers(base.Owner)
                     .Where(flower => flower.IsAlive && !before.Contains(flower)))
        {
            await PowerCmd.Apply<SunflowerFieldPower>(
                context,
                sunflower,
                DynamicVars["Heal"].BaseValue,
                base.Owner.Creature,
                this,
                false
            );
        }
    }

    protected override void OnUpgrade() => DynamicVars["Heal"].UpgradeValueBy(1m);
}
