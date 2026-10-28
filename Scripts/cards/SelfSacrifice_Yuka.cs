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
public sealed class SelfSacrifice_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("HpLoss", 9m),
        new EnergyVar(1),
        new DynamicVar("SunlightGain", 1m)
    ];
    public SelfSacrifice_Yuka() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.None, true) { }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        List<(Creature Flower, decimal Damage)> contributions = [];
        foreach (Creature flower in SunflowerManager.GetSunflowers(base.Owner).ToList())
        {
            if (!flower.IsAlive) continue;
            decimal damage = Math.Min(DynamicVars["HpLoss"].BaseValue, flower.CurrentHp);
            if (damage > 0m)
            {
                contributions.Add((flower, damage));
            }
        }

        decimal totalDamage = contributions.Sum(entry => entry.Damage);
        if (totalDamage <= 0m) return;

        foreach (var contribution in contributions)
        {
            await CreatureCmd.Damage(
                context,
                contribution.Flower,
                contribution.Damage,
                ValueProp.Unblockable | ValueProp.Unpowered,
                this,
                play
            );

            if (!contribution.Flower.IsAlive)
            {
                await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, base.Owner);
                SunlightManager.GainSunlight(base.Owner, DynamicVars["SunlightGain"].IntValue);
            }
        }

        foreach (var contribution in contributions)
        {
            Creature? target = GetLowestAliveEnemy();
            if (target == null)
            {
                break;
            }

            await CreatureCmd.Damage(
                context,
                target,
                contribution.Damage,
                ValueProp.Move,
                contribution.Flower,
                this,
                play
            );
        }
    }
    protected override void OnUpgrade() => base.AddKeyword(CardKeyword.Retain);
}
