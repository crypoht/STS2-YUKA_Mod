using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(KazamiYukaRelicPool))]
public sealed class PlantingGloves_Yuka : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Sunlight", 2m)
    ];

    private static string ResolvedIconPath =>
        ResourceLoader.Exists("res://mods/KazamiYuka/images/relics/PlantingGloves_Yuka.png")
            ? "res://mods/KazamiYuka/images/relics/PlantingGloves_Yuka.png"
            : "res://mods/KazamiYuka/images/relics/SunflowerHeadpiece_Yuka.png";

    public override RelicAssetProfile AssetProfile => new(
        IconPath: ResolvedIconPath,
        IconOutlinePath: ResolvedIconPath,
        BigIconPath: ResolvedIconPath
    );

    public void OnSunflowerPlanted()
    {
        Flash();
        SunlightManager.GainSunlight(Owner, DynamicVars["Sunlight"].IntValue);
    }
}
