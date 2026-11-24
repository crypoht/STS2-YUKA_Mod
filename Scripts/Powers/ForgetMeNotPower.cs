using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class ForgetMeNotPower : ModPowerTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxHpGain", 2m)
    ];

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
            await RemoveCards(
                base.Owner.Player,
                base.Amount,
                false,
                base.DynamicVars["MaxHpGain"].BaseValue);
        }
    }

    public static async Task RemoveCards(
        MegaCrit.Sts2.Core.Entities.Players.Player player,
        decimal amount,
        bool prioritizeLowerRarity,
        decimal maxHpGain)
    {
        if (amount <= 0m)
        {
            return;
        }

        for (int index = 0; index < (int)amount; index++)
        {
            List<CardModel> candidates = player.Deck.Cards
                .Where(card => card.IsRemovable && card is not ForgetMeNot_Yuka)
                .ToList();
            if (candidates.Count == 0)
            {
                break;
            }

            List<CardModel> eligibleCandidates = candidates;
            if (prioritizeLowerRarity)
            {
                int priority = candidates.Min(GetDeletionPriority);
                eligibleCandidates = candidates
                    .Where(card => GetDeletionPriority(card) == priority)
                    .ToList();
            }

            CardModel? selected = player.RunState.Rng.CombatCardSelection.NextItem(eligibleCandidates);

            if (selected == null)
            {
                break;
            }

            await CardPileCmd.RemoveFromDeck(selected, true);
            await CreatureCmd.GainMaxHp(player.Creature, maxHpGain);
        }
    }

    private static int GetDeletionPriority(CardModel card)
    {
        if (card.Rarity == CardRarity.Curse)
        {
            return 0;
        }

        if (card.Rarity == CardRarity.Basic &&
            (card.Tags.Contains(CardTag.Strike) || card.Tags.Contains(CardTag.Defend)))
        {
            return 1;
        }

        if (card.Rarity is CardRarity.Common or CardRarity.Basic)
        {
            return 2;
        }

        if (card.Rarity is CardRarity.Uncommon or CardRarity.Rare)
        {
            return 3;
        }

        return 4;
    }
}

[RegisterPower]
public sealed class ForgetMeNotUpgradedPower : ModPowerTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxHpGain", 2m)
    ];

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
            await ForgetMeNotPower.RemoveCards(
                base.Owner.Player,
                base.Amount,
                true,
                base.DynamicVars["MaxHpGain"].BaseValue);
        }
    }
}
