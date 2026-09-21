using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class HeatPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/HeatPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/HeatPower.png"
    );

    public override async Task BeforeSideTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != base.Owner.Side || !base.Owner.IsAlive || base.Owner.PetOwner == null)
        {
            return;
        }

        if (base.Owner.CombatState == null)
        {
            return;
        }

        base.Flash();
        foreach (Creature enemy in base.Owner.CombatState.Enemies.Where(enemy => enemy.IsAlive).ToList())
        {
            await CreatureCmd.Damage(
                choiceContext,
                enemy,
                base.Amount,
                ValueProp.Unpowered | ValueProp.Move,
                base.Owner
            );
        }

    }
}
