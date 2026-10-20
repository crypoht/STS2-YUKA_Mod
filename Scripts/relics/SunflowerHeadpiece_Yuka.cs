using System.Linq;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(KazamiYukaRelicPool))]
public sealed class SunflowerHeadpiece_Yuka : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/relics/SunflowerHeadpiece_Yuka.png",
        IconOutlinePath: "res://mods/KazamiYuka/images/relics/SunflowerHeadpiece_Yuka.png",
        BigIconPath: "res://mods/KazamiYuka/images/relics/SunflowerHeadpiece_Yuka.png"
    );

    public static decimal GetBonusMaxHp(Player player)
    {
        return player?.Relics?.Any(relic => relic is SunflowerHeadpiece_Yuka) == true ? 5m : 0m;
    }
}
