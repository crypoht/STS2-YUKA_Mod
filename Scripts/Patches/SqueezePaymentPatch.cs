using System;
using System.Threading.Tasks;
using HarmonyLib;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;

namespace KazamiYuka.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.CanPlay),
    new Type[] { typeof(UnplayableReason), typeof(AbstractModel) },
    new ArgumentType[] { ArgumentType.Out, ArgumentType.Out })]
public static class SqueezeCanPlayPatch
{
    public static void Postfix(
        CardModel __instance,
        ref bool __result,
        ref UnplayableReason reason)
    {
        if (__result || !SqueezeManager.CanUseFertilizerMode(__instance))
        {
            return;
        }

        reason &= ~(UnplayableReason.HasUnplayableKeyword |
            UnplayableReason.EnergyCostTooHigh |
            UnplayableReason.StarCostTooHigh |
            UnplayableReason.NoLivingAllies |
            UnplayableReason.BlockedByCardLogic);
        __result = reason == UnplayableReason.None;
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.CanPlayTargeting))]
public static class SqueezeCanPlayTargetingPatch
{
    [HarmonyPriority(Priority.First)]
    public static void Prefix(
        CardModel __instance,
        ref Creature? target,
        out SqueezeManager.TargetScope? __state)
    {
        if (SqueezeManager.IsSqueezed(__instance))
        {
            target = SqueezeManager.NormalizeOriginalTarget(__instance, target);
        }

        __state = SqueezeManager.EnterTarget(__instance, target);
    }

    public static void Finalizer(SqueezeManager.TargetScope? __state) => __state?.Dispose();
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.TryManualPlay))]
public static class SqueezeManualPlayTargetPatch
{
    public static void Prefix(CardModel __instance, ref Creature? target)
    {
        if (SqueezeManager.IsSqueezed(__instance))
        {
            target = SqueezeManager.NormalizeOriginalTarget(__instance, target);
        }
    }
}

[HarmonyPatch(typeof(PlayCardAction), "ExecuteAction")]
public static class SqueezeActionTargetPatch
{
    public static void Prefix(PlayCardAction __instance, out SqueezeManager.TargetScope? __state)
    {
        __state = SqueezeManager.EnterTarget(
            __instance.NetCombatCard.ToCardModelOrNull(), __instance.Target);
    }

    public static void Postfix(SqueezeManager.TargetScope? __state, ref Task __result)
    {
        if (__state != null)
        {
            __result = SqueezeManager.CompleteTargetScope(__result, __state);
        }
    }

}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
public static class SqueezePlayTargetPatch
{
    public static void Prefix(
        CardModel __instance,
        Creature? target,
        out SqueezeManager.TargetScope? __state)
    {
        __state = SqueezeManager.EnterTarget(__instance, target);
    }

    public static void Postfix(SqueezeManager.TargetScope? __state, ref Task __result)
    {
        if (__state != null)
        {
            __result = SqueezeManager.CompleteTargetScope(__result, __state);
        }
    }

}

[HarmonyPatch(typeof(CardCmd), nameof(CardCmd.AutoPlay))]
public static class SqueezeAutoPlayTargetPatch
{
    public static void Prefix(
        CardModel card,
        Creature? target,
        out SqueezeManager.TargetScope? __state)
    {
        __state = SqueezeManager.EnterTarget(card, target);
    }

    public static void Postfix(SqueezeManager.TargetScope? __state, ref Task __result)
    {
        if (__state != null)
        {
            __result = SqueezeManager.CompleteTargetScope(__result, __state);
        }
    }

}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.SpendResources))]
public static class SqueezeSpendResourcesPatch
{
    [HarmonyPriority(Priority.First)]
    public static bool Prefix(CardModel __instance, ref Task<(int, int)> __result)
    {
        if (!SqueezeManager.IsFertilizerPlay(__instance))
        {
            return true;
        }

        __result = Task.FromResult((0, 0));
        return false;
    }
}

[HarmonyPatch(typeof(CardModel), "GetResultLocationForCardPlay")]
public static class SqueezeExhaustResultPatch
{
    public static void Postfix(CardModel __instance, ref CardLocation __result)
    {
        if (SqueezeManager.IsFertilizerPlay(__instance))
        {
            __result = new CardLocation(__instance.Owner, PileType.Exhaust, CardPilePosition.Bottom);
        }
    }
}
