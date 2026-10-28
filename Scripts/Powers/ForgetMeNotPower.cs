using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class ForgetMeNotPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/ForgetMeNotPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/ForgetMeNotPower.png"
    );

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (base.Owner.Player != null)
        {
            await RemoveCards(base.Owner.Player, base.Amount, false);
        }
    }

    public static async Task RemoveCards(
        MegaCrit.Sts2.Core.Entities.Players.Player player,
        decimal amount,
        bool prioritizeLowerRarity)
    {
        if (amount <= 0m)
        {
            return;
        }

        for (int index = 0; index < (int)amount; index++)
        {
            List<CardModel> candidates = player.Deck.Cards
                .Where(card => card is not ForgetMeNot_Yuka)
                .ToList();
            if (candidates.Count == 0)
            {
                break;
            }

            IEnumerable<CardModel> weightedCandidates = candidates
                .Where(card => GetWeight(card, prioritizeLowerRarity) > 0);
            CardModel? selected = weightedCandidates.Any()
                ? player.RunState.Rng.CombatCardSelection.WeightedNextItem(
                    weightedCandidates,
                    card => GetWeight(card, prioritizeLowerRarity))
                : player.RunState.Rng.CombatCardSelection.NextItem(candidates);

            if (selected == null)
            {
                break;
            }

            await CardPileCmd.RemoveFromDeck(selected, true);
        }
    }

    private static float GetWeight(CardModel? card, bool prioritizeLowerRarity)
    {
        if (card == null)
        {
            return 0f;
        }

        float weight = card.Rarity switch
        {
            CardRarity.Basic => 40f,
            CardRarity.Curse => 100f,
            CardRarity.Common => 10f,
            CardRarity.Uncommon or CardRarity.Rare => 5f,
            _ => 1f
        };

        if (prioritizeLowerRarity && card.IsUpgraded)
        {
            weight = System.Math.Max(0f, weight - 5f);
        }

        return weight;
    }
}

[RegisterPower]
public sealed class ForgetMeNotUpgradedPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/ForgetMeNotPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/ForgetMeNotPower.png"
    );

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (base.Owner.Player != null)
        {
            await ForgetMeNotPower.RemoveCards(base.Owner.Player, base.Amount, true);
        }
    }
}
