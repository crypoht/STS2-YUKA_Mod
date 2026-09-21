using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class DoubleHeadedSunflower_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("ExtraXiangYang", 2m)
    ];

    public DoubleHeadedSunflower_Yuka()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal extraXiangYang = DynamicVars["ExtraXiangYang"].BaseValue;
        await PowerCmd.Apply<DoubleHeadedSunflowerPower>(
            choiceContext,
            base.Owner.Creature,
            extraXiangYang,
            base.Owner.Creature,
            this,
            false
        );

        foreach (var sunflower in SunflowerManager.GetSunflowers(base.Owner).Where(flower => flower.IsAlive))
        {
            await PowerCmd.Apply<XiangYangPower>(
                choiceContext,
                sunflower,
                extraXiangYang,
                base.Owner.Creature,
                this,
                false
            );
        }
    }

    protected override void OnUpgrade()
    {
        base.AddKeyword(CardKeyword.Innate);
    }
}
