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
using KazamiYuka.Scripts.Interfaces; 
using STS2RitsuLib.Keywords;
using KazamiYuka.Scripts.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class Shading_Yuka : KazamiYukaCard, IVariableSunlightCard
{
    public Shading_Yuka() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<MegaCrit.Sts2.Core.HoverTips.IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(KazamiYukaKeywords.FierceSun)
    ];

    public int GetMaxSunlightConsume() => (int)DynamicVars["MaxSunConsume"].BaseValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        // 👇 1. 返璞归真！直接用最基础的 BlockVar，彻底告别渲染崩溃！
        new BlockVar(7m, ValueProp.Move), 
        new DynamicVar("MaxSunConsume", 2m) 
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        if (player == null) return;

        // 2. 调用接口自动扣除阳光，并拿到实际消耗的次数
        int actualSpent = this.ConsumeVariableSunlight(player, cardPlay);

        if (actualSpent > 0)
        {
            // 👇 3. 核心改动：写一个 for 循环！消耗了几点阳光，就 GainBlock 几次
            for (int i = 0; i < actualSpent; i++)
            {
                await CreatureCmd.GainBlock(player.Creature, base.DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay, false);
                
                // 可选：加一点微小的延迟（0.1秒），让多次叠甲的“叮、叮”音效和数字跳动更有节奏感
                await Cmd.Wait(0.1f, false);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 👇 4. 升级也改回最简单的 Block 强化
        base.DynamicVars.Block.UpgradeValueBy(3m);
    }
}
