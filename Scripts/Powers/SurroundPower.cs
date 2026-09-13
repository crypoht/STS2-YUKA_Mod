using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class SurroundPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/SurroundPower_small.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/SurroundPower.png"
    );

    public static decimal GetBonusMaxHp(Player summoner)
    {
        if (summoner?.Creature == null || !summoner.Creature.HasPower<SurroundPower>())
        {
            return 0m;
        }

        return summoner.Creature.GetPower<SurroundPower>()?.Amount ?? 0m;
    }
}
