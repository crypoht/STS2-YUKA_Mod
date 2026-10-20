using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using KazamiYuka.Scripts;

namespace KazamiYuka.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.TargetType), MethodType.Getter)]
public static class SqueezeTargetTypePatch
{
    public static void Postfix(CardModel __instance, ref TargetType __result)
    {
        if (SqueezeManager.IsSqueezed(__instance))
        {
            if (!SqueezeManager.HasOriginalTargetType(__instance))
            {
                SqueezeManager.RememberOriginalTargetType(__instance, __result);
            }

            if (SqueezeManager.GetOriginalTargetType(__instance) ==
                    KazamiYukaTargetTypes.Sunflower ||
                !SqueezeManager.ShouldUseSqueezeTargetType(__instance))
            {
                return;
            }

            __result = KazamiYukaTargetTypes.SqueezedCardTarget;
        }
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.IsValidTarget))]
public static class SqueezeValidTargetPatch
{
    public static void Postfix(CardModel __instance, Creature target, ref bool __result)
    {
        if (SqueezeManager.IsSqueezed(__instance) &&
            SqueezeManager.ShouldUseSqueezeTargetType(__instance))
        {
            __result = SqueezeManager.IsValidOriginalOrSunflowerTarget(__instance, target);
        }
    }
}

[HarmonyPatch]
public static class SqueezeCardPlayPatch
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        MethodInfo? baseOnPlay = AccessTools.DeclaredMethod(typeof(CardModel), "OnPlay");
        if (baseOnPlay != null)
        {
            yield return baseOnPlay;
        }

        foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch
            {
                continue;
            }

            foreach (Type type in types.Where(type =>
                typeof(CardModel).IsAssignableFrom(type) &&
                !type.IsAbstract))
            {
                MethodInfo? onPlay = AccessTools.DeclaredMethod(type, "OnPlay");
                if (onPlay != null)
                {
                    yield return onPlay;
                }
            }
        }
    }

    public static bool Prefix(
        CardModel __instance,
        object[] __args,
        ref Task __result)
    {
        PlayerChoiceContext? choiceContext = __args
            .OfType<PlayerChoiceContext>()
            .FirstOrDefault();
        CardPlay? cardPlay = __args
            .OfType<CardPlay>()
            .FirstOrDefault();

        if (!SqueezeManager.IsSqueezed(__instance) ||
            choiceContext == null ||
            cardPlay == null ||
            !SqueezeManager.IsValidTarget(__instance, cardPlay.Target))
        {
            return true;
        }

        __result = SqueezeManager.PlayAsFertilizer(choiceContext, __instance, cardPlay);
        return false;
    }
}
