using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace KazamiYuka.Scripts.Patches;

public static class FreeCardTracking
{
    private sealed class Marker
    {
        public bool ThisTurn;
        public bool ThisCombat;
    }

    private static readonly ConditionalWeakTable<CardModel, Marker> Markers = new();

    public static void MarkThisTurn(CardModel card)
    {
        Markers.GetValue(card, _ => new Marker()).ThisTurn = true;
    }

    public static void MarkThisCombat(CardModel card)
    {
        Markers.GetValue(card, _ => new Marker()).ThisCombat = true;
    }

    public static bool IsMarked(CardModel card)
    {
        return Markers.TryGetValue(card, out Marker? marker)
            && (marker.ThisTurn || marker.ThisCombat);
    }

    public static void ClearThisTurn(CardModel card)
    {
        if (!Markers.TryGetValue(card, out Marker? marker))
        {
            return;
        }

        marker.ThisTurn = false;
        if (!marker.ThisCombat)
        {
            Markers.Remove(card);
        }
    }

    public static void ClearAll()
    {
        Markers.Clear();
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.SetToFreeThisTurn))]
public static class SetCardFreeThisTurnPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance)
    {
        FreeCardTracking.MarkThisTurn(__instance);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.SetToFreeThisCombat))]
public static class SetCardFreeThisCombatPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance)
    {
        FreeCardTracking.MarkThisCombat(__instance);
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class ClearTurnFreeCardAfterPlayedPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref Task __result, CardPlay cardPlay)
    {
        if (cardPlay.IsLastInSeries)
        {
            __result = ClearAfterCardPlay(__result, cardPlay.Card);
        }
    }

    private static async Task ClearAfterCardPlay(Task originalTask, CardModel card)
    {
        await originalTask;
        FreeCardTracking.ClearThisTurn(card);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.EndOfTurnCleanup))]
public static class ClearTurnFreeCardAtTurnEndPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance)
    {
        FreeCardTracking.ClearThisTurn(__instance);
    }
}

[HarmonyPatch(typeof(PlayerCombatState), nameof(PlayerCombatState.AfterCombatEnd))]
public static class ClearFreeCardMarkersAfterCombatPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        FreeCardTracking.ClearAll();
    }
}
