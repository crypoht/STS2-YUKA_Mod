using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class FantasyFortuneSlip_Yuka : YukaAncientRelic
{
    public override bool HasUponPickupEffect => true;

    public override Task AfterObtained() =>
        YukaRewardCardSelection.ChooseCards(Owner, new LocString("relics", Id.Entry + ".selectionScreenPrompt"));
}

internal static class YukaRewardCardSelection
{
    internal static async Task ChooseCards(Player owner, LocString prompt)
    {
        List<CardCreationResult> cards = ModelDb.CardPool<KazamiYukaCardPool>()
            .GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint)
            .Where(card => (card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare) &&
                card.IsAllowedInRun(owner.RunState))
            .DistinctBy(card => card.Id)
            .OrderBy(card => card.Id.Entry, StringComparer.Ordinal)
            .Select(card => new CardCreationResult(owner.RunState.CreateCard(card, owner)))
            .ToList();

        if (cards.Count == 0)
            return;

        CardSelectorPrefs preferences = new(prompt, 0, cards.Count);
        IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGridForRewards(
            new BlockingPlayerChoiceContext(), cards, owner, preferences);
        foreach (CardModel card in selected)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(
                card, PileType.Deck, CardPilePosition.Bottom));
        }
    }
}
