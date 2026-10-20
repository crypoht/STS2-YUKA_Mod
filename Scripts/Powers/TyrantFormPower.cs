using System;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class TyrantFormPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/TyrantFormPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/TyrantFormPower.png"
    );

    public static bool AllowsSunflowerTarget(CardModel? card, Creature? target)
    {
        if (card == null ||
            target?.Monster is not KazamiYukaSunflower ||
            !target.IsAlive ||
            card.Type != CardType.Attack ||
            card.Owner?.Creature.GetPower<TyrantFormPower>() == null)
        {
            return false;
        }

        return card.TargetType is TargetType.AnyEnemy or TargetType.RandomEnemy or TargetType.AllEnemies;
    }

    public static bool AllowsAttackCard(CardModel? card)
    {
        return card?.Owner?.Creature.GetPower<TyrantFormPower>() != null &&
            card.Type == CardType.Attack &&
            card.TargetType is TargetType.AnyEnemy or TargetType.RandomEnemy or TargetType.AllEnemies;
    }

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (!result.WasTargetKilled ||
            target.Monster is not KazamiYukaSunflower ||
            dealer?.Monster is KazamiYukaSunflower ||
            cardSource?.Type != CardType.Attack ||
            cardSource.Owner?.Creature != base.Owner ||
            target.MaxHp <= 0m)
        {
            return;
        }

        var combatState = base.Owner.CombatState;
        if (combatState == null)
        {
            return;
        }

        decimal damage = target.MaxHp * base.Amount;
        if (damage <= 0m)
        {
            return;
        }

        var enemies = combatState.Enemies
            .Where(enemy => enemy.IsAlive)
            .ToList();
        if (enemies.Count == 0)
        {
            return;
        }

        await CreatureCmd.Damage(
            choiceContext,
            enemies,
            damage,
            ValueProp.Unpowered | ValueProp.Move,
            base.Owner,
            null,
            null
        );
    }
}
