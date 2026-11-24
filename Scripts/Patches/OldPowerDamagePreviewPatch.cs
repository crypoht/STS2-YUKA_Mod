using System.Linq;
using HarmonyLib;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.UpdateDynamicVarPreview))]
public static class OldPowerDamagePreviewPatch
{
    public static void Postfix(
        CardModel __instance,
        CardPreviewMode previewMode,
        Creature? target,
        DynamicVarSet dynamicVarSet)
    {
        if (__instance.IsCanonical ||
            !CombatManager.Instance.IsInProgress ||
            __instance.CombatState == null ||
            __instance.Owner?.Creature.GetPower<OldPower>() is not { Amount: > 0 } ||
            !OldPower.AppliesTo(__instance) ||
            __instance.Pile?.Type is PileType.Hand or PileType.Play ||
            __instance.UpgradePreviewType == CardUpgradePreviewType.Combat)
        {
            return;
        }

        foreach (DynamicVar variable in dynamicVarSet.Values.ToList())
        {
            if (variable is DamageVar or CalculatedDamageVar ||
                variable.Name is "SunflowerDamage" or "TotalDamage")
            {
                variable.UpdateCardPreview(__instance, previewMode, target, true);
            }
        }
    }
}
