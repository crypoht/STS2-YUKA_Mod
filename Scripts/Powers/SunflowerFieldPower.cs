using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class SunflowerFieldPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/SunflowerFieldPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/SunflowerFieldPower.png"
    );

    public static async Task OnSunflowerHealthChanged(
        PlayerChoiceContext context,
        Creature sunflower,
        FlowerHealthEvent eventType,
        decimal amount)
    {
        if (eventType != FlowerHealthEvent.Damage || amount <= 0m || !sunflower.IsAlive)
        {
            return;
        }

        SunflowerFieldPower? power = sunflower.GetPower<SunflowerFieldPower>();
        if (power == null || power.Amount <= 0m)
        {
            return;
        }

        await SunflowerManager.HealSingle(context, sunflower, power.Amount);
    }
}
