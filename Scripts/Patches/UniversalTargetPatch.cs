using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using KazamiYuka.Scripts;

namespace KazamiYuka.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.IsValidTarget))]
public class UniversalTargetPatch
{
    public static void Postfix(CardModel __instance, Creature target, ref bool __result)
    {

        if (!__result || target == null)
        {
            return;
        }

        if (__instance is ICustomTargetValidator customValidator)
        {
            __result = customValidator.IsValidCustomTarget(target);
        }
    }
}