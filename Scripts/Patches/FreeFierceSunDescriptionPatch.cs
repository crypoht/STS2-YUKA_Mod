using System;
using System.Reflection;
using HarmonyLib;
using KazamiYuka.Scripts.Cards;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch]
public static class FreeFierceSunDescriptionPatch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        Type previewType = typeof(CardModel).GetNestedType("DescriptionPreviewType", BindingFlags.NonPublic)
            ?? throw new MissingMemberException(typeof(CardModel).FullName, "DescriptionPreviewType");
        return AccessTools.Method(typeof(CardModel), nameof(CardModel.GetDescriptionForPile),
            new[] { typeof(PileType), previewType, typeof(Creature) })
            ?? throw new MissingMethodException(typeof(CardModel).FullName, nameof(CardModel.GetDescriptionForPile));
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(CardModel __instance, ref string __result)
    {
        if (__instance is not KazamiYukaCard card
            || card.IsCanonical
            || card.HasBeenRemovedFromState
            || !card.Keywords.Contains(KazamiYukaKeywords.FierceSun)
            || !card.IsSunlightFree)
        {
            return;
        }

        __result = __result.Replace(
            "[gold]烈阳[/gold]",
            "[yuka_rainbow_still]烈阳[/yuka_rainbow_still]");
    }
}
