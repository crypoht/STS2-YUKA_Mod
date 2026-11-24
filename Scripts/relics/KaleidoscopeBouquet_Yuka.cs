using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class KaleidoscopeBouquet_Yuka : YukaAncientRelic
{
    private bool _usedThisCombat;

    public override Task BeforeCombatStart()
    {
        _usedThisCombat = false;
        return Task.CompletedTask;
    }

    public override async Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || _usedThisCombat || player.Creature.IsDead ||
            player.Creature.CombatState is not { } combatState)
            return;

        _usedThisCombat = true;
        CardMultiplayerConstraint constraint = player.RunState.CardMultiplayerConstraint;
        IEnumerable<CardModel> candidates = ModelDb.AllCharacterCardPools
            .Append(ModelDb.CardPool<ColorlessCardPool>())
            .SelectMany(pool => pool.GetUnlockedCards(player.UnlockState, constraint))
            .Where(card => card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare)
            .Concat(ModelDb.AllCards.Where(card => card.Rarity == CardRarity.Ancient))
            .Where(card => card.Type == CardType.Power && card.CanBeGeneratedInCombat &&
                card.IsAllowedInRun(player.RunState) && !card.CanonicalKeywords.Contains(CardKeyword.Unplayable))
            .DistinctBy(card => card.Id)
            .OrderBy(card => card.Id.Entry, StringComparer.Ordinal);
        CardModel[] selected = candidates.TakeRandom(3, player.RunState.Rng.CombatCardGeneration).ToArray();

        if (selected.Length == 0)
            return;

        Flash();
        foreach (CardModel model in selected)
        {
            if (CombatManager.Instance.IsOverOrEnding || player.Creature.IsDead)
                break;

            CardModel card = combatState.CreateCard(model, player);
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
    }
}
