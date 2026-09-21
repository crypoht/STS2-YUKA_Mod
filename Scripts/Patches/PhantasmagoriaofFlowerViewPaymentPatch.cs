using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using KazamiYuka.Scripts.Cards;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(PlayerCombatState), nameof(PlayerCombatState.HasEnoughResourcesFor))]
public static class PhantasmagoriaofFlowerViewCanPlayPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel card, ref bool __result, ref UnplayableReason reason)
    {
        if (__result ||
            !reason.HasFlag(UnplayableReason.EnergyCostTooHigh) ||
            !PhantasmagoriaofFlowerViewPayment.TryCreatePlan(card, out _))
        {
            return;
        }

        reason &= ~UnplayableReason.EnergyCostTooHigh;
        __result = reason == UnplayableReason.None;
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.SpendResources))]
public static class PhantasmagoriaofFlowerViewSpendResourcesPatch
{
    [HarmonyPrefix]
    public static void Prefix(CardModel __instance, out PhantasmagoriaofFlowerViewPayment.Plan? __state)
    {
        __state = PhantasmagoriaofFlowerViewPayment.TryCreatePlan(__instance, out var plan)
            ? plan
            : null;
    }

    [HarmonyPostfix]
    public static void Postfix(
        CardModel __instance,
        PhantasmagoriaofFlowerViewPayment.Plan? __state,
        ref Task<(int, int)> __result)
    {
        if (__state != null)
        {
            __result = PhantasmagoriaofFlowerViewPayment.PayAfterEnergy(__instance, __state, __result);
        }
    }
}

public static class PhantasmagoriaofFlowerViewPayment
{
    private sealed record FixedSunlightPayment(int Amount);

    public sealed record Plan(Player Player, int FixedSunlight, int ReplacementSunlight);

    private static readonly ConditionalWeakTable<CardModel, FixedSunlightPayment> FixedPayments = new();

    public static bool TryCreatePlan(CardModel? card, out Plan? plan)
    {
        plan = null;
        if (card == null || card.IsCanonical || card.EnergyCost.CostsX)
        {
            return false;
        }

        Player? player = card.Owner;
        if (player == null || player.Creature == null)
        {
            return false;
        }

        PhantasmagoriaofFlowerViewPower? power =
            player.Creature.GetPower<PhantasmagoriaofFlowerViewPower>();
        if (power == null || power.Amount <= 0m || player.PlayerCombatState == null)
        {
            return false;
        }

        int energyCost = card.EnergyCost.GetAmountToSpend();
        int missingEnergy = Math.Max(0, energyCost - player.PlayerCombatState.Energy);
        if (missingEnergy <= 0)
        {
            return false;
        }

        int fixedSunlight = GetFixedSunlightCost(card);
        int sunlightPerEnergy = Math.Max(0, (int)power.Amount);
        int replacementSunlight = checked(missingEnergy * sunlightPerEnergy);
        int totalSunlight = checked(fixedSunlight + replacementSunlight);
        if (!SunlightManager.HasEnoughSunlight(player, totalSunlight))
        {
            return false;
        }

        plan = new Plan(player, fixedSunlight, replacementSunlight);
        return true;
    }

    public static bool TryConsumeFixedSunlightPayment(CardModel card, int expectedAmount)
    {
        if (!FixedPayments.TryGetValue(card, out FixedSunlightPayment? payment))
        {
            return false;
        }

        FixedPayments.Remove(card);
        return payment.Amount == expectedAmount;
    }

    public static async Task<(int, int)> PayAfterEnergy(
        CardModel card,
        Plan plan,
        Task<(int, int)> original)
    {
        (int energySpent, int starsSpent) = await original;

        if (plan.FixedSunlight > 0)
        {
            if (!SunlightManager.TrySpendSunlight(plan.Player, plan.FixedSunlight))
            {
                return (energySpent, starsSpent);
            }

            FixedPayments.Remove(card);
            FixedPayments.Add(card, new FixedSunlightPayment(plan.FixedSunlight));
        }

        if (plan.ReplacementSunlight > 0)
        {
            SunlightManager.TrySpendSunlight(plan.Player, plan.ReplacementSunlight);
        }

        return (energySpent, starsSpent);
    }

    private static int GetFixedSunlightCost(CardModel card)
    {
        if (card is not KazamiYukaCard yukaCard ||
            KazamiYukaCard.IsEnergyCostTemporarilyFree(card) ||
            !yukaCard.DynamicVars.TryGetValue("SunlightCost", out var sunlightCost))
        {
            return 0;
        }

        return Math.Max(0, decimal.ToInt32(sunlightCost.BaseValue));
    }
}
