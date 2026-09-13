using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;


namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class Unity_Yuka : KazamiYukaCard
{
    public override STS2RitsuLib.Scaffolding.Content.CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        // 👇 1. 直接把官方基数当作我们的“基础成长值”，图鉴会完美读取并显示它！
        new CalculationBaseVar(2m), 
        new CalculationExtraVar(1m),
        
        // 👇 2. 计算公式
        new CalculatedVar("CalculatedGrow").WithMultiplier((card, _) => {
            var player = card.Owner;
            if (player == null) return 0m; 

            int flowerCount = SunflowerManager.GetSunflowers(player).Count;
            
            decimal baseGrow = card.DynamicVars["CalculationBase"].BaseValue;
            

            decimal targetTotal = baseGrow * (decimal)Math.Pow(2, flowerCount);
            
            return targetTotal - baseGrow;
        })
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(KazamiYukaKeywords.Grow)
    ];

    public Unity_Yuka() : base(2, CardType.Skill, CardRarity.Common, TargetType.None, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner;
        if (player == null) return;

        int finalGrow = (int)((CalculatedVar)DynamicVars["CalculatedGrow"]).Calculate(null);

        await SunflowerManager.Summon(player, finalGrow);
    }

    protected override void OnUpgrade()
    {

        DynamicVars["CalculationBase"].UpgradeValueBy(1m);
    }
}
