using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.ValueProps;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class UmbrellaParry_Yuka : KazamiYukaCard // 继承自您专属的基类
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(8, ValueProp.Move),                   // 基础 8 点格挡
        new DynamicVar("SunlightGain", 1m)    // 基础获得 1 点阳光
    ];

    // 1费，技能牌，普通稀有度，目标为自身
    public UmbrellaParry_Yuka() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int blockAmount = (int)DynamicVars.Block.BaseValue;
        int sunAmount = (int)DynamicVars["SunlightGain"].BaseValue;

        // 1. 执行官方标准获得格挡指令
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false);

        // 2. 完美的获取阳光
        SunlightManager.GainSunlight(base.Owner, sunAmount);
    }

    protected override void OnUpgrade()
    {
        // 升级时：格挡保持 8 点，阳光获得量 +1（从 1 变成 2）
        DynamicVars["SunlightGain"].UpgradeValueBy(1m);
    }
}
