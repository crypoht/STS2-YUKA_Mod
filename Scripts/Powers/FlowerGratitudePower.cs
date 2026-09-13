using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class FlowerGratitudePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/FlowerGratitudePower_small.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/FlowerGratitudePower.png"
    );

    // 能力被触发时的抽卡逻辑
    public async Task TriggerDraw(PlayerChoiceContext choiceContext)
    {
        if (base.Owner.Player == null) return;
        await CardPileCmd.Draw(choiceContext, base.Amount, base.Owner.Player, false);
    }

    // =========================================================
    // 👇 全局监听器：不管场上发生什么，只要是向日葵血量变了，就会通知这里
    // =========================================================
    public static async Task OnAnySunflowerHealthChanged(PlayerChoiceContext ctx, Creature sunflower, FlowerHealthEvent eventType, decimal amount)
    {
        // 我们这张卡只关心【生命回复】和【上限增加】，如果是扣血则直接无视
        if (eventType != FlowerHealthEvent.Heal && eventType != FlowerHealthEvent.MaxHpIncrease)
        {
            return;
        }

        // 确认向日葵的主人 (玩家)
        var player = sunflower.PetOwner?.Creature;
        
        // 检查玩家身上是否有“花的报恩”这个状态
        if (player != null && player.HasPower<FlowerGratitudePower>())
        {
            var power = player.GetPower<FlowerGratitudePower>();
            if (power != null)
            {
                await power.TriggerDraw(ctx);
            }
        }
    }
}