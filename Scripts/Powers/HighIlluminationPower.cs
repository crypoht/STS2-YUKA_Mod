using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class HighIlluminationPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/HighIlluminationPower_small.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/HighIlluminationPower.png"
    );

    public override async Task BeforeSideTurnEndEarly(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != base.Owner.Side || !base.Owner.IsAlive || base.Owner.PetOwner == null)
        {
            return;
        }

        var combatState = base.Owner.CombatState;
        if (combatState == null)
        {
            return;
        }

        var lowestHpEnemy = combatState.Enemies
            .Where(enemy => enemy.IsAlive)
            .OrderBy(enemy => enemy.CurrentHp)
            .FirstOrDefault();

        if (lowestHpEnemy != null)
        {
            base.Flash();
            await CreatureCmd.Damage(
                choiceContext,
                lowestHpEnemy,
                base.Amount,
                ValueProp.Unpowered | ValueProp.Move,
                base.Owner,
                null,
                null
            );
        }
    }
}
