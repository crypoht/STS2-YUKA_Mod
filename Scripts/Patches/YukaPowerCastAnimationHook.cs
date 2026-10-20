using System.Threading.Tasks;
using KazamiYuka.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Cards;

namespace KazamiYuka.Scripts.Patches;

public sealed class YukaPowerCastAnimationHook : ICardOnPlayHookListener
{
    public async Task<bool> BeforeCardOnPlay(BeforeCardOnPlayContext context)
    {
        if (context.CardPlay.Card is not KazamiYukaCard card || card.Type != CardType.Power)
        {
            return false;
        }

        if (card.Owner is not { } owner)
        {
            return false;
        }

        await CreatureCmd.TriggerAnim(owner.Creature, "Cast", owner.Character.CastAnimDelay);
        return false;
    }
}
