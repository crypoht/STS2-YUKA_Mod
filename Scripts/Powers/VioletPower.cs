using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class VioletPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/VioletPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/VioletPower.png"
    );

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != base.Owner.Side ||
            !base.Owner.IsAlive ||
            base.Amount <= 0m)
        {
            return;
        }

        base.Flash();
        PlayerChoiceContext choiceContext = new ThrowingPlayerChoiceContext();
        foreach (var combatPlayer in combatState.Players)
        {
            await CardPileCmd.Draw(choiceContext, base.Amount, combatPlayer, false);
        }
    }
}
