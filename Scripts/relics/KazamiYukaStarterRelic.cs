using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Rooms;
using KazamiYuka.Scripts.Powers;
using KazamiYuka.Scripts.Relics;

namespace KazamiYuka.Scripts;


[RegisterRelic(typeof(KazamiYukaRelicPool))]
[RegisterCharacterStarterRelic(typeof(KazamiYukaCharacter))] 
[RegisterTouchOfOrobasRefinement(typeof(OldSleepingCap_Yuka))]
public class KazamiYukaStarterRelic : ModRelicTemplate
{

    public override bool ShowCounter => true;

    public override int DisplayAmount =>
        SunflowerManager.GetRelicXiangYangApplicationsRemaining(base.Owner);

    public void RefreshSunflowerCounter()
    {
        InvokeDisplayAmountChanged();
    }

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://mods/KazamiYuka/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"res://mods/KazamiYuka/images/relics/{GetType().Name}.png",
        BigIconPath: $"res://mods/KazamiYuka/images/relics/{GetType().Name}.png"
    );

public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            base.Flash();
            
            if (SunlightManager.IsYukaPlayer(base.Owner))
            {
                SunlightManager.GainSunlight(base.Owner, 5);
            }
        }
    }

}
