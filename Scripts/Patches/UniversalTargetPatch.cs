using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.Powers;

namespace KazamiYuka.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.IsValidTarget))]
public class UniversalTargetPatch
{
    public static void Postfix(CardModel __instance, Creature target, ref bool __result)
    {

        if (target == null)
        {
            return;
        }

        if (TyrantFormPower.AllowsSunflowerTarget(__instance, target))
        {
            __result = true;
            return;
        }

        if (!__result)
        {
            return;
        }

        if (__instance is ICustomTargetValidator customValidator)
        {
            __result = customValidator.IsValidCustomTarget(target);
        }
    }
}
