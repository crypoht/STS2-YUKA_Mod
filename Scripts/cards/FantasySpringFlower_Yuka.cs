using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class FantasySpringFlower_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Summon", 8m),
        new DynamicVar("SpringAmount", 2m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [KazamiYukaKeywords.Grow];

    public FantasySpringFlower_Yuka() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var before = SunflowerManager.GetSunflowers(base.Owner).ToHashSet();
        await SunflowerManager.Summon(base.Owner, DynamicVars["Summon"].BaseValue);
        var summoned = SunflowerManager.GetSunflowers(base.Owner).FirstOrDefault(flower => !before.Contains(flower));
        if (summoned != null)
        {
            await PowerCmd.Apply<SpringFlowerPower>(
                context, summoned, DynamicVars["SpringAmount"].BaseValue, base.Owner.Creature, this, false);
        }
    }

    protected override void OnUpgrade() => DynamicVars["SpringAmount"].UpgradeValueBy(1m);
}
