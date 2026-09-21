using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using KazamiYuka.Scripts.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class FertilizerMaking_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    protected override IEnumerable<MegaCrit.Sts2.Core.HoverTips.IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromCard<Fertilizer_Yuka>(base.IsUpgraded)
    ];
    public override IEnumerable<MegaCrit.Sts2.Core.Entities.Cards.CardKeyword> CanonicalKeywords => [
        MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Exhaust
    ];
    protected override IEnumerable<MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar> CanonicalVars => [
        new SunlightCostVar(1m)
    ];

    public FertilizerMaking_Yuka()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (!TryPayFixedSunlight(base.Owner, DynamicVars["SunlightCost"].IntValue, play))
        {
            return;
        }

        foreach (PileType pileType in new[] { PileType.Hand, PileType.Draw, PileType.Discard })
        {
            var pile = pileType.GetPile(base.Owner);
            foreach (CardModel card in pile.Cards
                .Where(card => card.Type == CardType.Status && card.IsTransformable)
                .ToList())
            {
                CardModel fertilizer = CombatState!.CreateCard<Fertilizer_Yuka>(base.Owner);
                if (base.IsUpgraded)
                {
                    CardCmd.Upgrade(fertilizer, CardPreviewStyle.HorizontalLayout);
                }

                await CardCmd.Transform(card, fertilizer, CardPreviewStyle.HorizontalLayout);
            }
        }
    }
}
