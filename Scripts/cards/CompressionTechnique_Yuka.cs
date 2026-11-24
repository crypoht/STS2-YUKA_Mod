using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class CompressionTechnique_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("CardCount", 1m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromCard<Fertilizer_Yuka>(IsUpgraded)
    ];

    public CompressionTechnique_Yuka()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsUpgraded)
        {
            await PowerCmd.Apply<CompressionTechniqueUpgradedPower>(
                choiceContext, Owner.Creature, DynamicVars["CardCount"].BaseValue,
                Owner.Creature, this, false);
        }
        else
        {
            await PowerCmd.Apply<CompressionTechniquePower>(
                choiceContext, Owner.Creature, DynamicVars["CardCount"].BaseValue,
                Owner.Creature, this, false);
        }
    }



    protected override void OnUpgrade()
    {
    }
}
