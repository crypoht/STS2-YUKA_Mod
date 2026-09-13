using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
// 确保引用了 SunlightManager 所在的命名空间
using KazamiYuka.Scripts; 
using KazamiYuka.Scripts.DynamicVars;

namespace KazamiYuka.Scripts.Cards; // 根据你的项目路径调整命名空间

// 注册卡牌到幽香的专属卡池 (请将 KazamiYukaCardPool 替换为你实际定义的卡池类名)
[RegisterCard(typeof(KazamiYukaCardPool))]
public class SunlightStrike_Yuka : KazamiYukaCard
{
    // 卡图资源配置 (RitsuLib 独有写法)
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    // 卡牌基础数值
    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10m, ValueProp.Move),
         new SunlightCostVar(1m),
    ];

    public SunlightStrike_Yuka() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    // 打出时的效果逻辑
    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        if (player == null) return;

        // 👇 核心修改：读取动态变量，并把 cardPlay 收据交给基类查验！
        int requiredSunlight = (int)DynamicVars["SunlightCost"].BaseValue;
        if (base.TryPayFixedSunlight(player, requiredSunlight, cardPlay))
        {
            // 只有成功扣除阳光（或被判定为免费），才会执行伤害指令
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_starry_impact", null, null) // 这里暂时借用原版星星撞击特效
                .Execute(choiceContext);
        }
        else
        {
            // 如果玩家阳光不够，强行打出这张牌不会产生伤害
            // 可以在这里加一行提示音效，比如：
            // await SfxCmd.Play("event:/sfx/ui/error", 1f);
        }
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        // 升级时增加 4 点伤害（变成 14 点）
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
