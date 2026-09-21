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

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == base.Owner.Player)
        {
            base.Flash();

            SunlightManager.GainSunlight(player, 4);
            
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
