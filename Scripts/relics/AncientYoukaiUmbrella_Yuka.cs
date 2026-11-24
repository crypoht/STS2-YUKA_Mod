using KazamiYuka.Scripts.Patches;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class AncientYoukaiUmbrella_Yuka : YukaAncientRelic
{
    private const int HitLimit = 5;
    private int _hits;

    public override bool ShowCounter => true;
    public override int DisplayAmount => _hits;

    public override Task BeforeCombatStart()
    {
        ResetCounter();
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player && participants.Contains(Owner.Creature))
            ResetCounter();

        return Task.CompletedTask;
    }

    public override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        return target == Owner.Creature && CombatManager.Instance.IsInProgress && _hits >= HitLimit
            ? 0m
            : decimal.MaxValue;
    }

    internal void CountUnblockedHit(PlayerChoiceContext choiceContext, DamageResult result,
        Creature? dealer, CardModel? cardSource)
    {
        Creature target = result.Receiver;
        if (target != Owner.Creature || !CombatManager.Instance.IsInProgress ||
            result.UnblockedDamage <= 0 || _hits >= HitLimit)
            return;

        PowerModel? power = choiceContext.LastInvolvedModel as PowerModel;
        if (power == null && AncientUmbrellaDamageSourcePatch.PoisonSource is { } poison && poison.Owner == target)
            power = poison;
        if (!IsQualifyingDamage(target, dealer, cardSource, power))
            return;

        _hits++;
        InvokeDisplayAmountChanged();
        if (_hits == HitLimit)
            Flash();
    }

    private static bool IsQualifyingDamage(Creature target, Creature? dealer, CardModel? cardSource, PowerModel? power)
    {
        return cardSource?.Type == CardType.Status ||
            dealer != null && dealer.Side != target.Side ||
            cardSource == null && power?.TypeForCurrentAmount == PowerType.Debuff;
    }

    private void ResetCounter()
    {
        _hits = 0;
        InvokeDisplayAmountChanged();
    }
}
