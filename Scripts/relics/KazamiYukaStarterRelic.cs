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

namespace KazamiYuka.Scripts;


[RegisterRelic(typeof(KazamiYukaRelicPool))]
[RegisterCharacterStarterRelic(typeof(KazamiYukaCharacter))] 
public class KazamiYukaStarterRelic : ModRelicTemplate
{

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://mods/KazamiYuka/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"res://mods/KazamiYuka/images/relics/{GetType().Name}.png",
        BigIconPath: $"res://mods/KazamiYuka/images/relics/{GetType().Name}.png"
    );

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        // 确保只有进入“战斗房间”时才触发
        if (room is CombatRoom)
        {
            base.Flash(); // 让遗物在右上角闪烁并跳动一下，给玩家视觉反馈
            
            // 直接调用你写好的全局阳光管理器
            SunlightManager.GainSunlight(base.Owner,3);
        }
    }

}