using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class DoubleHeadedSunflowerPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/DoubleHeadedSunflowerPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/DoubleHeadedSunflowerPower.png"
    );

    public static decimal GetExtraXiangYang(Player player)
    {
        if (player?.Creature == null)
        {
            return 0m;
        }

        return player.Creature.GetPower<DoubleHeadedSunflowerPower>()?.Amount ?? 0m;
    }
}
