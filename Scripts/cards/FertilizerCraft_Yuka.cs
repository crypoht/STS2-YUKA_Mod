using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class FertilizerCraft_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromCard<Fertilizer_Yuka>(base.IsUpgraded)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new SunlightCostVar(2m)
    ];

    public FertilizerCraft_Yuka()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (!TryPayFixedSunlight(
                base.Owner,
                DynamicVars["SunlightCost"].IntValue,
                cardPlay))
        {
            return;
        }

        foreach (Player ally in base.CombatState?.Players
            .Where(player => player != base.Owner)
            .ToList() ?? [])
        {
            foreach (PileType pileType in new[] { PileType.Hand, PileType.Draw, PileType.Discard })
            {
                var pile = pileType.GetPile(ally);
                foreach (CardModel card in pile.Cards
                    .Where(card => card.Type == CardType.Status && card.IsTransformable)
                    .ToList())
                {
                    CardModel fertilizer = CombatState!.CreateCard<Fertilizer_Yuka>(ally);
                    if (base.IsUpgraded)
                    {
                        CardCmd.Upgrade(fertilizer, CardPreviewStyle.HorizontalLayout);
                    }

                    await CardCmd.Transform(card, fertilizer, CardPreviewStyle.HorizontalLayout);
                }
            }
        }
    }
}
