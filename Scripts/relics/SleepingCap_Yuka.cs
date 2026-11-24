using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class SleepingCap_Yuka : YukaAncientRelic
{
    private bool _playedCard;
    private bool _healed;
    private int _consecutiveEmptyTurns;

    public override bool ShowCounter => !_healed;
    public override int DisplayAmount => _consecutiveEmptyTurns;

    public override Task BeforeCombatStart()
    {
        _playedCard = false;
        _healed = false;
        _consecutiveEmptyTurns = 0;
        Status = RelicStatus.Normal;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player && participants.Contains(Owner.Creature))
            _playedCard = false;

        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner)
            _playedCard = true;

        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (_healed || side != CombatSide.Player || !participants.Contains(Owner.Creature) || Owner.Creature.IsDead)
            return;

        _consecutiveEmptyTurns = _playedCard ? 0 : _consecutiveEmptyTurns + 1;
        if (_consecutiveEmptyTurns < 2)
        {
            InvokeDisplayAmountChanged();
            return;
        }

        _healed = true;
        InvokeDisplayAmountChanged();
        Flash();
        Status = RelicStatus.Disabled;
        await CreatureCmd.Heal(Owner.Creature, Owner.Creature.MaxHp - Owner.Creature.CurrentHp);
    }
}
