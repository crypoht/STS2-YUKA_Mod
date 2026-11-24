using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(CurseCardPool))]
public sealed class Daisy_Yuka : KazamiYukaCard
{
    public override bool CanBeGeneratedByModifiers => false;
    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Unplayable, CardKeyword.Eternal, CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    public Daisy_Yuka() : base(-1, CardType.Curse, CardRarity.Curse, TargetType.None, true)
    {
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || cardPlay.Card.Type != CardType.Attack ||
            Pile?.Type != PileType.Hand || CombatState is not { } combatState)
            return;

        if (CombatManager.Instance.History.CardPlaysFinished
            .Where(entry => entry.CardPlay.Player == Owner && entry.CardPlay.Card.Type == CardType.Attack &&
                entry.HappenedThisTurn(combatState))
            .Take(2)
            .Count() == 2)
            await CardCmd.Exhaust(choiceContext, this);
    }

    public override Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player) =>
        player == Owner && Pile?.Type == PileType.Exhaust
            ? PlayerCmd.LoseEnergy(1m, player)
            : Task.CompletedTask;

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Retain);
}
