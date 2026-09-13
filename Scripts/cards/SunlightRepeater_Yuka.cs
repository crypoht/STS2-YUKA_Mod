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
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS2RitsuLib.Keywords;
using KazamiYuka.Scripts.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class SunlightRepeater_Yuka : KazamiYukaCard
{
    public SunlightRepeater_Yuka() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

     protected override IEnumerable<MegaCrit.Sts2.Core.HoverTips.IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(KazamiYukaKeywords.FierceSun)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            DynamicVar[] array = new DynamicVar[4];
            array[0] = new CalculationBaseVar(8m);      
            array[1] = new ExtraDamageVar(4m);          
            array[2] = new DynamicVar("MaxSunConsume", 8m); 
            
            array[3] = new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, MegaCrit.Sts2.Core.Entities.Creatures.Creature? _) => 
            {
                int maxSunLimit = (int)card.DynamicVars["MaxSunConsume"].BaseValue;

                // 👇 核心改动 1：UI 视觉同步！
                // 读取官方底层数据：如果卡牌现在处于免费状态，面板直接按满额阳光计算伤害！
                bool isFree = false;
                if (card.EnergyCost != null && card.EnergyCost.GetResolved() == 0)
                {
                    if (card.EnergyCost.Canonical > 0 || card.EnergyCost.HasLocalModifiers) isFree = true;
                }
                
                if (isFree) return maxSunLimit;

                // 如果没免费，再老老实实看池子里有多少阳光
                return Math.Min(SunlightManager.CurrentSunlight, maxSunLimit);
            });

            return array;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        if (player == null) return;
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        int maxSunLimit = (int)DynamicVars["MaxSunConsume"].BaseValue;

        // 👇 核心改动 2：删掉原本手动计算和直接调用 SunlightManager 的代码！
        // 调用基类的终极接口，交出 cardPlay 收据，让基类去判断是不是免费！
        int actualSpent = base.ConsumeUpToMaxSunlight(player, maxSunLimit, cardPlay);

        // 因为我们在上面的 UI 计算里加了 isFree 判断
        // 所以当免费时，此时读取的 PreviewValue 绝对是满额的最大伤害！
        decimal finalDamage = base.DynamicVars.CalculatedDamage.PreviewValue;

        // 轰出最终伤害！
        await DamageCmd.Attack(finalDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_starry_impact", null, null)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 升级时：阳光限制到 10，系数到 5
        DynamicVars["MaxSunConsume"].UpgradeValueBy(2m);
        base.DynamicVars.ExtraDamage.UpgradeValueBy(1m); // 4 -> 5
    }
}
