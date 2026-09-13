using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
// 确保引入你的 SunlightManager 所在的命名空间
using KazamiYuka.Scripts; 

namespace KazamiYuka.Scripts.Cards;

// 👇 应用了你分享的最新卡池注册语法！
[RegisterCard(typeof(KazamiYukaCardPool))] 
public class SunlightCollection_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        // 基础阳光获取量为 3
        new DynamicVar("SunAmount", 3m)
    ];

    // 0费，技能牌，普通稀有度，无目标
    public SunlightCollection_Yuka() : base(0, CardType.Skill, CardRarity.Common, TargetType.None, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获取动态变量里的阳光数值
        int amount = (int)DynamicVars["SunAmount"].BaseValue;

        // 👇 核心修复：base.Owner 就是 Player 本身，直接传进去即可！
        SunlightManager.GainSunlight(base.Owner, amount);

        // 同步方法转异步的占位符
        await Task.CompletedTask; 
    }

    protected override void OnUpgrade()
    {
        // 升级时：数值 +2（变成 5）
        DynamicVars["SunAmount"].UpgradeValueBy(2m);
    }
}
