using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using KazamiYuka.Scripts.Cards;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(NHandCardHolder))]
public static class FreeFierceSunGlowPatch
{
    private sealed class GlowState
    {
        public required NCard Card;
        public required Tween Tween;
    }

    private static readonly ConditionalWeakTable<NHandCardHolder, GlowState> States = new();

    [HarmonyPatch(nameof(NHandCardHolder.UpdateCard))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void UpdatePostfix(NHandCardHolder __instance)
    {
        NCard? cardNode = __instance.CardNode;
        if (!__instance.IsNodeReady()
            || cardNode?.CardHighlight == null
            || cardNode.Model is not KazamiYukaCard card
            || !CombatManager.Instance.IsInProgress
            || (!card.Keywords.Contains(KazamiYukaKeywords.FierceSun)
                && !card.Keywords.Contains(KazamiYukaKeywords.FierceSunInfinite))
            || !card.IsSunlightFree
            || !card.CanPlay())
        {
            Stop(__instance);
            return;
        }

        if (States.TryGetValue(__instance, out GlowState? state)
            && state.Card == cardNode
            && state.Tween.IsValid())
        {
            return;
        }

        Stop(__instance);
        cardNode.CardHighlight.Modulate = Color.FromHsv(0f, 0.72f, 1f, 0.98f);
        Tween tween = __instance.CreateTween().SetLoops();
        tween.TweenMethod(Callable.From<float>(hue =>
        {
            if (GodotObject.IsInstanceValid(cardNode)
                && GodotObject.IsInstanceValid(cardNode.CardHighlight))
            {
                cardNode.CardHighlight.Modulate = Color.FromHsv(hue % 1f, 0.72f, 1f, 0.98f);
            }
        }), 0f, 1f, 3.125);
        States.Add(__instance, new GlowState { Card = cardNode, Tween = tween });
    }

    [HarmonyPatch(nameof(NHandCardHolder.Clear))]
    [HarmonyPrefix]
    public static void ClearPrefix(NHandCardHolder __instance) => ReleaseHolderGlow(__instance);

    [HarmonyPatch(nameof(NHandCardHolder._ExitTree))]
    [HarmonyPrefix]
    public static void ExitTreePrefix(NHandCardHolder __instance) => ReleaseHolderGlow(__instance);

    private static void ReleaseHolderGlow(NHandCardHolder __instance)
    {
        if (States.TryGetValue(__instance, out GlowState? state)
            && GodotObject.IsInstanceValid(state.Card)
            && GodotObject.IsInstanceValid(state.Card.CardHighlight))
        {
            state.Card.CardHighlight.Modulate = NCardHighlight.playableColor;
        }

        Stop(__instance);
    }

    private static void Stop(NHandCardHolder __instance)
    {
        if (!States.TryGetValue(__instance, out GlowState? state))
        {
            return;
        }

        if (state.Tween.IsValid())
        {
            state.Tween.Kill();
        }

        States.Remove(__instance);
    }
}
