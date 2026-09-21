using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
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
public sealed class SelfSacrifice_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("HpLoss", 8m)
    ];
    public SelfSacrifice_Yuka() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.None, true) { }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        Creature? target = CombatState?.Enemies.Where(enemy => enemy.IsAlive).OrderBy(enemy => enemy.CurrentHp).FirstOrDefault();
        if (target == null) return;

        List<(Creature Flower, decimal Damage)> contributions = [];
        foreach (Creature flower in SunflowerManager.GetSunflowers(base.Owner).ToList())
        {
            if (!flower.IsAlive) continue;
            decimal before = flower.CurrentHp;
            await CreatureCmd.Damage(context, flower, DynamicVars["HpLoss"].BaseValue, ValueProp.Unblockable | ValueProp.Unpowered, flower, this, play);
            if (flower.IsAlive)
            {
                contributions.Add((flower, before - flower.CurrentHp));
            }
        }

        decimal totalDamage = contributions.Sum(entry => entry.Damage);
        if (totalDamage <= 0m) return;

        foreach (var contribution in contributions.Where(entry => entry.Damage > 0m))
        {
            await CreatureCmd.Damage(context, target, contribution.Damage, ValueProp.Unpowered | ValueProp.Move, contribution.Flower, this, play);
        }
    }
    protected override void OnUpgrade() => base.EnergyCost.UpgradeBy(-1);
}
