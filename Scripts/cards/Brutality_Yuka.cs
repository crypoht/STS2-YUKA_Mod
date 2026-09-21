using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class Brutality_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) =>
        {
            var player = card.Owner;
            if (player == null)
            {
                return 0m;
            }

            decimal totalDamage = 0m;
            foreach (Creature sunflower in SunflowerManager.GetSunflowers(player))
            {
                totalDamage += Math.Ceiling(sunflower.CurrentHp / 2m);
            }

            return totalDamage * card.DynamicVars["DamageMultiplier"].BaseValue;
        }),
        new DynamicVar("DamageMultiplier", 2m)
    ];

    public Brutality_Yuka() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner;
        if (player == null)
        {
            return;
        }

        decimal totalDamage = DynamicVars.CalculatedDamage.Calculate(null);
        foreach (Creature sunflower in SunflowerManager.GetSunflowers(player))
        {
            int hpLoss = (int)Math.Ceiling(sunflower.CurrentHp / 2m);
            if (hpLoss <= 0)
            {
                continue;
            }

            await CreatureCmd.Damage(
                choiceContext,
                sunflower,
                hpLoss,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                this,
                cardPlay
            );
        }

        if (totalDamage <= 0)
        {
            return;
        }

        var combatState = base.CombatState;
        if (combatState == null)
        {
            return;
        }

        await DamageCmd.Attack(totalDamage)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(combatState)
            .WithHitFx("vfx/vfx_attack_blunt", null, null)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
