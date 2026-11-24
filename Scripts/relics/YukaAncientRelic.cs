using Godot;
using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Relics;

public abstract class YukaAncientRelic : ModRelicTemplate
{
    private const string IconDirectory = "res://mods/KazamiYuka/images/relics";

    public sealed override RelicRarity Rarity => RelicRarity.Ancient;

    public sealed override RelicAssetProfile AssetProfile
    {
        get
        {
            string iconPath = $"{IconDirectory}/{GetType().Name}.png";
            if (!ResourceLoader.Exists(iconPath))
                iconPath = $"{IconDirectory}/temp.png";

            return new(IconPath: iconPath, IconOutlinePath: iconPath, BigIconPath: iconPath);
        }
    }
}
