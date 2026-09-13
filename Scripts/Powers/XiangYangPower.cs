using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower] 
public class XiangYangPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/xiangyang_power.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/xiangyang_power_large.png"
    );

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (base.Owner != null && side == base.Owner.Side)
        {
            base.Flash(); 
            

            if (base.Owner.PetOwner is { } yukaPlayer)
            {
                SunlightManager.GainSunlight(yukaPlayer, base.Amount); 
            }
            else if (base.Owner.Player is { } selfPlayer)
            {
                SunlightManager.GainSunlight(selfPlayer, base.Amount);
            }
        }
    }

}
