using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(KazamiYukaRelicPool))]
public sealed class YoukaiUmbrella_Yuka : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/relics/YoukaiUmbrella_Yuka.png",
        IconOutlinePath: "res://mods/KazamiYuka/images/relics/YoukaiUmbrella_Yuka.png",
        BigIconPath: "res://mods/KazamiYuka/images/relics/YoukaiUmbrella_Yuka.png"
    );
}
