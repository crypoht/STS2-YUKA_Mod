using System.Threading;
using HarmonyLib;
using KazamiYuka.Scripts.Relics;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(PoisonPower), nameof(PoisonPower.AfterSideTurnStart))]
internal static class AncientUmbrellaDamageSourcePatch
{
    private static readonly AsyncLocal<PoisonPower?> Source = new();

    internal static PoisonPower? PoisonSource => Source.Value;

    private static void Prefix(PoisonPower __instance, out PoisonPower? __state)
    {
        __state = Source.Value;
        Source.Value = __instance;
    }

    private static void Finalizer(PoisonPower? __state) => Source.Value = __state;
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterDamageGiven))]
internal static class AncientUmbrellaDamageCountPatch
{
    private static void Prefix(PlayerChoiceContext choiceContext, DamageResult results,
        Creature? dealer, CardModel? cardSource)
    {
        if (results.TotalDamage > 0 &&
            results.Receiver.Player?.GetRelic<AncientYoukaiUmbrella_Yuka>() is { IsMelted: false } umbrella)
            umbrella.CountHit(choiceContext, results, dealer, cardSource);
    }
}
