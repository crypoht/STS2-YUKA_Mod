using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class CompoundFertilizer_Yuka : KazamiYukaCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(KazamiYukaKeywords.Squeeze),
        ModKeywordRegistry.CreateHoverTip(KazamiYukaKeywords.SunflowerCard)
    ];

    public CompoundFertilizer_Yuka()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> cards = new[] { PileType.Draw, PileType.Hand, PileType.Discard }
            .SelectMany(pileType => pileType.GetPile(Owner).Cards)
            .Where(card => card.Type != CardType.Attack &&
                !SunflowerCardTags.IsSunflowerCard(card) &&
                !SqueezeManager.IsSqueezed(card))
            .ToList();

        foreach (CardModel card in cards)
        {
            SqueezeManager.RememberOriginalTargetType(card, card.TargetType);
            CardCmd.ApplyKeyword(card, KazamiYukaKeywords.Squeeze);
        }

        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
