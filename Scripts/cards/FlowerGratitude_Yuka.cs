using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts.Powers;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.DynamicVars;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class FlowerGratitude_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DrawAmount", 1m), // 每次触发抽1张
        new SunlightCostVar(10m),
    ];

    // 2费，能力牌，罕见，目标是自己
    public FlowerGratitude_Yuka() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 👇 修复 1：使用标准安全的 as 转换，彻底消灭 CS1061 报错
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        if (player == null) return;

        // 👇 核心修改：读取动态变量，并把 cardPlay 收据交给基类查验！
        int requiredSunlight = (int)DynamicVars["SunlightCost"].BaseValue;
        if (base.TryPayFixedSunlight(player, requiredSunlight, cardPlay))
        {
            int amount = (int)DynamicVars["DrawAmount"].BaseValue;

            // 给自己挂上“花的报恩”能力
            await PowerCmd.Apply<FlowerGratitudePower>(
                choiceContext,
                base.Owner.Creature,
                amount,
                base.Owner.Creature,
                this,
                false
            );
        }
        else
        {
            // 阳光不足时，卡牌直接打出但不生效（这里做保险）
            // 可以加一个音效提示失败
        }
    }

    protected override void OnUpgrade() => base.EnergyCost.UpgradeBy(-1);
}
