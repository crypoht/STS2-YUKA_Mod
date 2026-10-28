using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class FlowerMoundPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/FlowerMoundPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/FlowerMoundPower.png"
    );

    public static async Task OnAnySunflowerHealthChanged(
        MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext ctx,
        Creature sunflower,
        FlowerHealthEvent eventType,
        decimal amount)
    {
        if (eventType != FlowerHealthEvent.Damage || amount <= 0m) return;

        foreach (var player in SunflowerManager.GetCombatPlayers(sunflower))
        {
            FlowerMoundPower? power = player.Creature.GetPower<FlowerMoundPower>();
            if (power != null)
            {
                SunlightManager.GainSunlight(player, power.Amount);
                await PlayerCmd.GainEnergy(1m, player);
            }
        }

    }
}
