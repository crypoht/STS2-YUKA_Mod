using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace KazamiYuka.Scripts.Patches;

public static class FreeCardTracking
{
    private sealed class Marker;
    private static readonly ConditionalWeakTable<CardModel, Marker> Markers = new();

    public static void Mark(CardModel card)
    {
        Markers.GetValue(card, _ => new Marker());
    }

    public static bool IsMarked(CardModel card)
    {
        return Markers.TryGetValue(card, out _);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.SetToFreeThisTurn))]
public static class SetCardFreeThisTurnPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance)
    {
        FreeCardTracking.Mark(__instance);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.SetToFreeThisCombat))]
public static class SetCardFreeThisCombatPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance)
    {
        FreeCardTracking.Mark(__instance);
    }
}
