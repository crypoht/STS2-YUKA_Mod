using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class SunflowerCrown_Yuka : YukaAncientRelic
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Heal", 3)];

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner != Owner.Creature || Owner.Creature.IsDead || (int)amount == 0 ||
            power.GetTypeForAmount(amount) != PowerType.Debuff ||
            !IsExternalDebuffSource(choiceContext, applier, cardSource))
            return;

        Flash();
        await CreatureCmd.Heal(Owner.Creature, DynamicVars["Heal"].BaseValue);
    }

    private bool IsExternalDebuffSource(PlayerChoiceContext choiceContext, Creature? applier, CardModel? cardSource)
    {
        if (cardSource?.Owner == Owner)
            return false;

        AbstractModel? sourceModel = choiceContext.LastInvolvedModel ??
            (choiceContext as HookPlayerChoiceContext)?.Source;

        return sourceModel switch
        {
            PowerModel sourcePower => IsTeammate(sourcePower.Owner) || IsEnemy(sourcePower.Owner),
            CardModel sourceCard => IsTeammate(sourceCard.Owner?.Creature),
            MonsterModel sourceMonster => IsEnemy(sourceMonster.Creature),
            null when cardSource != null => IsTeammate(cardSource.Owner?.Creature),
            null => IsEnemy(applier),
            _ => false
        };
    }

    private bool IsTeammate(Creature? source) =>
        source != null && source != Owner.Creature && source.Side == Owner.Creature.Side && source.Player != null;

    private bool IsEnemy(Creature? source) => source != null && source.Side != Owner.Creature.Side;
}
