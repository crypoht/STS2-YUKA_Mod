using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class SunflowerCrown_Yuka : YukaAncientRelic
{
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        Creature? source = applier ?? cardSource?.Owner?.Creature;
        if (power.Owner != Owner.Creature || source == null || source == Owner.Creature ||
            (int)amount == 0 || power.GetTypeForAmount(amount) != PowerType.Debuff || Owner.Creature.IsDead)
            return;

        Flash();
        await CreatureCmd.Heal(Owner.Creature, 3m);
    }
}
