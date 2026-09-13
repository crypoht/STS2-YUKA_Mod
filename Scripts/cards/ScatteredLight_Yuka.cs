using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.Powers;
using STS2RitsuLib.Keywords;
using KazamiYuka.Scripts.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class ScatteredLight_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxConsume", 4m),     // 最多消耗 4 点
        new DynamicVar("GainPerConsume", 4m)  // 每消耗 1 点，每回合获得 4 点（仅用于文本显示）
    ];

     protected override IEnumerable<MegaCrit.Sts2.Core.HoverTips.IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(KazamiYukaKeywords.FierceSun)
    ];

    public ScatteredLight_Yuka() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        if (player == null) return;

        int maxConsume = (int)DynamicVars["MaxConsume"].BaseValue;

        // 调用底层接口扣费并查验免单
        // 调用底层接口扣费并查验免单
        int consumedSunlight = base.ConsumeUpToMaxSunlight(player, maxConsume, cardPlay);

        // 如果成功消耗了阳光（比如消耗了 3 点）
        if (consumedSunlight > 0)
        {
            // 👇 直接把消耗的阳光点数当作层数（回合数）发给玩家，不要乘 4！
            await PowerCmd.Apply<ScatteredLightPower>(
                choiceContext,
                player.Creature,
                consumedSunlight, 
                player.Creature,
                this,
                false
            );
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
