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
using STS2RitsuLib.Keywords;
using KazamiYuka.Scripts.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class TyrantObliteration_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7m, ValueProp.Move),         // 基础伤害 8
        new DynamicVar("MaxConsume", 2m),          // 最多消耗 4 点阳光
        new DynamicVar("HitCount", 1m)             // 初始攻击次数 1
    ];

     protected override IEnumerable<MegaCrit.Sts2.Core.HoverTips.IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(KazamiYukaKeywords.FierceSun)
    ];

    public TyrantObliteration_Yuka() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        if (player == null) return;
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        int maxConsume = (int)DynamicVars["MaxConsume"].BaseValue;

        int consumedSunlight = base.ConsumeUpToMaxSunlight(player, maxConsume, cardPlay);


        if (consumedSunlight > 0)
        {
            DynamicVars["HitCount"].BaseValue += consumedSunlight;
        }

        // 读取强化后的最终攻击次数
        int totalHits = (int)DynamicVars["HitCount"].BaseValue;

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .WithHitCount(totalHits) 
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash", null, null)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
        DynamicVars["MaxConsume"].UpgradeValueBy(2m);
    }
}
