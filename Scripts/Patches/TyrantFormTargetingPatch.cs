using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace KazamiYuka.Patches;

[HarmonyPatch(typeof(NTargetManager), "AllowedToTargetCreature")]
public static class TyrantFormTargetingUiPatch
{
    public static void Postfix(Creature creature, ref bool __result)
    {
        CardModel? card = SunflowerManager.GetActiveTargetingCard();
        if (TyrantFormPower.AllowsSunflowerTarget(card, creature))
        {
            __result = true;
        }
    }
}

[HarmonyPatch(typeof(AttackCommand), "GetPossibleTargets")]
public static class TyrantFormAttackTargetsPatch
{
    public static void Postfix(AttackCommand __instance, ref IReadOnlyList<Creature> __result)
    {
        if (__instance.ModelSource is not CardModel card ||
            !TyrantFormPower.AllowsAttackCard(card) ||
            __instance.Attacker?.Monster is KazamiYukaSunflower ||
            __instance.IsSingleTargeted)
        {
            return;
        }

        if (__instance.Attacker == null)
        {
            return;
        }

        var sunflowers = SunflowerManager.GetAllCombatSunflowers(__instance.Attacker);
        if (sunflowers.Count == 0)
        {
            return;
        }

        __result = __result
            .Concat(sunflowers)
            .Distinct()
            .ToList();
    }
}
