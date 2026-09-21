using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class RoseWarPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/RoseWarPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/RoseWarPower.png"
    );

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != base.Owner || result.BlockedDamage <= 0 || !props.IsPoweredAttack())
        {
            return;
        }

        if (base.Owner.Player is not { } player)
        {
            return;
        }

        foreach (Creature sunflower in SunflowerManager.GetSunflowers(player).Where(flower => flower.IsAlive).ToList())
        {
            await SunflowerManager.HealSingle(choiceContext, sunflower, result.BlockedDamage * base.Amount);
        }
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(base.Owner))
        {
            await PowerCmd.Decrement(this);
        }
    }
}
