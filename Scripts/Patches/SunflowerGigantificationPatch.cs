using System.Collections.Concurrent;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models.Powers;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed))]
public static class SunflowerGigantificationBeforeCardPlayedPatch
{
    private static readonly ConcurrentDictionary<CardPlay, decimal> AmountBeforePlay = new();

    [HarmonyPrefix]
    public static void Prefix(CardPlay cardPlay)
    {
        if (cardPlay.Card.Type != CardType.Attack || !SunflowerCardTags.IsSunflowerCard(cardPlay.Card))
        {
            return;
        }

        GigantificationPower? power = cardPlay.Player.Creature.GetPower<GigantificationPower>();
        if (power != null)
        {
            AmountBeforePlay[cardPlay] = power.Amount;
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
    public static class AfterCardPlayedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref Task __result, CardPlay cardPlay)
        {
            if (!AmountBeforePlay.TryRemove(cardPlay, out decimal amountBeforePlay))
            {
                return;
            }

            __result = DecrementIfAttackHookDidNotConsume(__result, cardPlay, amountBeforePlay);
        }

        private static async Task DecrementIfAttackHookDidNotConsume(
            Task originalTask,
            CardPlay cardPlay,
            decimal amountBeforePlay)
        {
            await originalTask;
            if (!cardPlay.IsLastInSeries)
            {
                return;
            }

            GigantificationPower? power = cardPlay.Player.Creature.GetPower<GigantificationPower>();
            if (power != null && power.Amount >= amountBeforePlay)
            {
                await PowerCmd.Decrement(power);
            }
        }
    }
}
