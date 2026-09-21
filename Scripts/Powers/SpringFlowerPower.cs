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

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class SpringFlowerPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/SpringFlowerPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/SpringFlowerPower.png"
    );

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != base.Owner.Side || !base.Owner.IsAlive || base.Owner.PetOwner == null)
        {
            return;
        }

        base.Flash();
        foreach (Creature flower in SunflowerManager.GetSunflowers(base.Owner.PetOwner)
                     .Where(flower => flower.IsAlive && flower != base.Owner)
                     .ToList())
        {
            await SunflowerManager.GainMaxHpSingle(null!, flower, base.Amount);
        }
    }
}
