using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using KazamiYuka.Scripts.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class TerrorAura_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(10m, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new DynamicVar("DebuffIncrease", 1m),
        new SunlightCostVar(1m)
    ];

    public TerrorAura_Yuka() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (!TryPayFixedSunlight(base.Owner, DynamicVars["SunlightCost"].IntValue, play))
        {
            return;
        }

        await CreatureCmd.GainBlock(base.Owner.Creature, DynamicVars.Block, play, false);

        foreach (var enemy in base.CombatState?.HittableEnemies.Where(enemy => enemy != null && !enemy.IsDead) ?? Enumerable.Empty<MegaCrit.Sts2.Core.Entities.Creatures.Creature>())
        {
            foreach (var power in enemy.Powers.Where(power => power.TypeForCurrentAmount == PowerType.Debuff).ToList())
            {
                power.SetAmount(power.Amount + DynamicVars["DebuffIncrease"].IntValue);
            }
        }
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(4m);
}
