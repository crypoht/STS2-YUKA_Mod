using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using KazamiYuka.Scripts.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class FacingSun_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(20m, ValueProp.Move),
        new SunlightCostVar(3m)
    ];

    public FacingSun_Yuka() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner;
        int sunlightCost = DynamicVars["SunlightCost"].IntValue;
        if (player == null || !TryPayFixedSunlight(player, sunlightCost, cardPlay))
        {
            return;
        }

        await CreatureCmd.GainBlock(
            player.Creature,
            DynamicVars.Block.BaseValue,
            DynamicVars.Block.Props,
            cardPlay,
            false
        );

        // 将临时能力挂在玩家身上，而不是逐朵挂在向日葵身上。
        // 这样既能稳定收到玩家回合开始钩子，也能在伤害修正中覆盖所有向日葵。
        await PowerCmd.Apply<FacingSunPower>(
            choiceContext,
            player.Creature,
            1m,
            player.Creature,
            this,
            false
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}
