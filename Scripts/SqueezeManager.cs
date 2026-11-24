using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Runtime.CompilerServices;
using KazamiYuka.Scripts.Keywords;
using KazamiYuka.Scripts.Powers;
using System;
using System.Linq;

namespace KazamiYuka.Scripts;

public static class SqueezeManager
{
    private static readonly ConditionalWeakTable<CardModel, OriginalTargetTypeHolder> OriginalTargetTypes = [];
    private static readonly ConditionalWeakTable<CardModel, TargetScope> TargetScopes = [];

    public static bool IsSqueezed(CardModel? card) =>
        card?.Keywords.Contains(KazamiYukaKeywords.Squeeze) == true;

    public static void RememberOriginalTargetType(CardModel card, TargetType targetType)
    {
        OriginalTargetTypes.GetValue(card, _ => new OriginalTargetTypeHolder()).Value = targetType;
    }

    public static bool HasOriginalTargetType(CardModel card) =>
        OriginalTargetTypes.TryGetValue(card, out _);

    public static TargetType GetOriginalTargetType(CardModel card)
    {
        if (!HasOriginalTargetType(card))
        {
            _ = card.TargetType;
        }

        return OriginalTargetTypes.TryGetValue(card, out OriginalTargetTypeHolder? holder)
            ? holder.Value
            : TargetType.None;
    }

    public static bool ShouldUseSqueezeTargetType(CardModel card)
    {
        return GetOriginalTargetType(card) != KazamiYukaTargetTypes.Sunflower;
    }

    public static bool UsesOriginalTargetType(CardModel card) =>
        TargetScopes.TryGetValue(card, out TargetScope? scope) &&
        !IsValidTarget(card, scope.Target);

    public static bool IsFertilizerPlay(CardModel card) =>
        TargetScopes.TryGetValue(card, out TargetScope? scope) &&
        IsValidTarget(card, scope.Target);

    public static bool CanUseFertilizerMode(CardModel card)
    {
        if (!IsSqueezed(card) || card.Owner?.PlayerCombatState == null ||
            !card.Owner.Creature.IsAlive)
        {
            return false;
        }

        return TargetScopes.TryGetValue(card, out TargetScope? scope)
            ? IsValidTarget(card, scope.Target)
            : SunflowerManager.GetSunflowers(card.Owner).Any(flower => flower.IsAlive);
    }

    public static Creature? NormalizeOriginalTarget(CardModel card, Creature? target)
    {
        TargetType originalTargetType = GetOriginalTargetType(card);
        return target == card.Owner?.Creature &&
            originalTargetType is TargetType.None or TargetType.Self or
                TargetType.AllEnemies or TargetType.RandomEnemy or TargetType.AllAllies
            ? null
            : target;
    }

    public static TargetScope? EnterTarget(CardModel? card, Creature? target) =>
        card != null && IsSqueezed(card) ? new TargetScope(card, target) : null;

    public static async Task CompleteTargetScope(Task original, TargetScope scope)
    {
        using (scope)
        {
            await original;
        }
    }

    public static bool IsValidOriginalOrSunflowerTarget(CardModel? card, Creature? target)
    {
        if (!IsSqueezed(card) || card == null)
        {
            return false;
        }

        if (target?.IsAlive == false)
        {
            return false;
        }

        if (IsValidTarget(card, target))
        {
            return true;
        }

        Creature? originalTarget = NormalizeOriginalTarget(card, target);
        using TargetScope? scope = EnterTarget(card, originalTarget);
        return card.IsValidTarget(originalTarget);
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
            await PowerCmd.Apply<XiangYangPower>(
                choiceContext, target, 1m, card.Owner.Creature, card, false);
        }
    }

    public sealed class TargetScope : IDisposable
    {
        private readonly CardModel _card;
        private readonly TargetScope? _previous;
        private bool _disposed;

        public Creature? Target { get; }

        public TargetScope(CardModel card, Creature? target)
        {
            _card = card;
            Target = target;
            TargetScopes.TryGetValue(card, out _previous);
            TargetScopes.Remove(card);
            TargetScopes.Add(card, this);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            TargetScopes.Remove(_card);
            if (_previous != null)
            {
                TargetScopes.Add(_card, _previous);
            }
        }
    }

    private sealed class OriginalTargetTypeHolder
    {
        public TargetType Value;
    }
}
