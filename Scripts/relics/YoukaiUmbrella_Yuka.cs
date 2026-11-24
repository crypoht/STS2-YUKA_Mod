using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(KazamiYukaRelicPool))]
public sealed class YoukaiUmbrella_Yuka : ModRelicTemplate
{
    private int _storedSunlight;

    [SavedProperty]
    public int StoredSunlight
    {
        get => _storedSunlight;
        set
        {
            AssertMutable();
            _storedSunlight = value;
        }
    }

    public override Task AfterObtained()
    {
        StoredSunlight = SunlightManager.GetSunlight(Owner);
        return Task.CompletedTask;
    }

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/relics/YoukaiUmbrella_Yuka_outline.png",
        IconOutlinePath: "res://mods/KazamiYuka/images/relics/YoukaiUmbrella_Yuka.png",
        BigIconPath: "res://mods/KazamiYuka/images/relics/YoukaiUmbrella_Yuka.png"
    );
}
