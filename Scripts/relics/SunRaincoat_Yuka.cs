using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(KazamiYukaRelicPool))]
public sealed class SunRaincoat_Yuka : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("ExtraXiangYang", 1m)
    ];

    private static string ResolvedIconPath =>
        ResourceLoader.Exists("res://mods/KazamiYuka/images/relics/SunRaincoat_Yuka.png")
            ? "res://mods/KazamiYuka/images/relics/SunRaincoat_Yuka.png"
            : "res://mods/KazamiYuka/images/relics/KazamiYukaStarterRelic.png";

    private const string OutlinePath = "res://mods/KazamiYuka/images/relics/SunRaincoat_Yuka_outline.png";

    public override RelicAssetProfile AssetProfile => new(
        IconPath: ResourceLoader.Exists(OutlinePath) ? OutlinePath : ResolvedIconPath,
        IconOutlinePath: ResolvedIconPath,
        BigIconPath: ResolvedIconPath
    );

    public static decimal GetExtraXiangYang(Player player) =>
        player.Relics.OfType<SunRaincoat_Yuka>()
            .Sum(relic => relic.DynamicVars["ExtraXiangYang"].BaseValue);
}
