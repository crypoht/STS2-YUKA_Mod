using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class SleepingTerror_Yuka : KazamiYukaCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<HibernatePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(6m),
        new DynamicVar("MaxHpGain", 4m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public SleepingTerror_Yuka() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        await CreatureCmd.Heal(base.Owner.Creature, DynamicVars.Heal.BaseValue, true);
        foreach (var flower in SunflowerManager.GetSunflowers(base.Owner).Where(flower => flower.IsAlive).ToList())
        {
            await SunflowerManager.GainMaxHpSingle(context, flower, DynamicVars["MaxHpGain"].BaseValue);
            await CreatureCmd.Heal(flower, flower.MaxHp, true);
            await PowerCmd.Apply<HibernatePower>(context, flower, 1m, base.Owner.Creature, this, false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(2m);
        DynamicVars["MaxHpGain"].UpgradeValueBy(1m);
    }
}
