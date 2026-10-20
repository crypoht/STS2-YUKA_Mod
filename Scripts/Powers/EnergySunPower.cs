using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class EnergySunPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/EnergySunPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/EnergySunPower.png"
    );

    public override async Task AfterEnergySpent(CardModel card, int amount)
    {
        if (amount > 0 && card.Owner.Creature == base.Owner)
        {
            if (base.Owner.Player != null)
            {
                SunlightManager.GainSunlight(base.Owner.Player, (int)base.Amount);
            }
            await Task.CompletedTask;
        }
    }
}
