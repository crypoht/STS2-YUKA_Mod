using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Runtime.CompilerServices;
using KazamiYuka.Scripts.Keywords;

namespace KazamiYuka.Scripts;

public static class SqueezeManager
{
    private static readonly ConditionalWeakTable<CardModel, OriginalTargetTypeHolder> OriginalTargetTypes = [];

    public static bool IsSqueezed(CardModel? card) =>
        card?.Keywords.Contains(KazamiYukaKeywords.Squeeze) == true;

    public static void RememberOriginalTargetType(CardModel card, TargetType targetType)
    {
        OriginalTargetTypes.GetValue(card, _ => new OriginalTargetTypeHolder()).Value = targetType;
    }

    public static bool HasOriginalTargetType(CardModel card) =>
        OriginalTargetTypes.TryGetValue(card, out _);

    public static TargetType GetOriginalTargetType(CardModel card) =>
        OriginalTargetTypes.TryGetValue(card, out OriginalTargetTypeHolder? holder)
            ? holder.Value
            : TargetType.None;

    public static bool ShouldUseSqueezeTargetType(CardModel card)
    {
        TargetType targetType = GetOriginalTargetType(card);
        return targetType is TargetType.None or
            TargetType.Self or
            TargetType.AnyEnemy or
            TargetType.AnyPlayer or
            TargetType.AnyAlly;
    }

    public static bool IsValidOriginalOrSunflowerTarget(CardModel? card, Creature? target)
    {
        if (!IsSqueezed(card) || card == null)
        {
            return false;
        }

        TargetType originalTargetType = GetOriginalTargetType(card);
        if (target == null)
        {
            return originalTargetType != TargetType.AnyEnemy &&
                   originalTargetType != TargetType.AnyAlly &&
                   originalTargetType != TargetType.AnyPlayer;
        }

        if (!target.IsAlive)
        {
            return false;
        }

        if (IsValidTarget(card, target))
        {
            return true;
        }

        return originalTargetType switch
        {
            TargetType.Self => target == card.Owner?.Creature,
            TargetType.AnyEnemy => target.Side != card.Owner?.Creature.Side,
            TargetType.AnyAlly => target.Side == card.Owner?.Creature.Side && target != card.Owner.Creature,
            TargetType.AnyPlayer => target.IsPlayer,
            _ => false
        };
    }

    public static bool IsValidTarget(CardModel? card, Creature? target) =>
        IsSqueezed(card) &&
        target is { IsAlive: true, Monster: KazamiYukaSunflower };

    public static async Task PlayAsFertilizer(
        PlayerChoiceContext choiceContext,
        CardModel card,
        CardPlay cardPlay)
    {
        if (cardPlay.Target is { IsAlive: true, Monster: KazamiYukaSunflower } target)
        {
            await SunflowerManager.GainMaxHpSingle(choiceContext, target, 3m);
            await CardCmd.Exhaust(choiceContext, card, false, false);
        }
    }

    private sealed class OriginalTargetTypeHolder
    {
        public TargetType Value;
    }
}
