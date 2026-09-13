using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public class ScatteredLightPower : ModPowerTemplate
{
    // 明确告诉引擎这是一个正面 Buff
    public override PowerType Type => PowerType.Buff;
    
    // 明确告诉引擎层数是可以叠加/减少的数字 (Counter)
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/ScatteredLightPower_small.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/ScatteredLightPower.png"
    );

    public ScatteredLightPower()
    {
    }

    // 完美使用你发来的官方回合开始接口
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 确保是玩家自己的回合开始
        if (player == base.Owner.Player)
        {
            // 闪烁一下能力图标，提示玩家生效了
            base.Flash();
            
            // 1. 发放固定的 4 点阳光
            SunlightManager.GainSunlight(player, 4);
            
            // 2. 层数减少 1 层
            // 在 STS2 的指令系统里，挂载负数层数（-1）会自动触发引擎的抵消逻辑。
            // 当层数被减到 0 的时候，引擎会自动帮你把这个 Buff 从状态栏彻底移除，非常安全！
            await PowerCmd.Apply<ScatteredLightPower>(
                choiceContext,
                base.Owner, 
                -1, 
                base.Owner, 
                null, 
                false
            );
        }
    }
}
