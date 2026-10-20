using Godot;
using HarmonyLib;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;

namespace KazamiYuka.Patches;

[HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder.UpdateCard))]
public static class SqueezeCardGlowPatch
{
    private static readonly Color SqueezeGlow = new(0.2f, 1f, 0.35f, 0.98f);

    [HarmonyPostfix]
    public static void Postfix(NHandCardHolder __instance)
    {
        NCard? cardNode = __instance.CardNode;
        if (cardNode?.Model == null ||
            cardNode.CardHighlight == null ||
            !CombatManager.Instance.IsInProgress ||
            !cardNode.Model.Keywords.Contains(KazamiYukaKeywords.Squeeze))
        {
            return;
        }

        cardNode.CardHighlight.AnimShow();
        cardNode.CardHighlight.Modulate = SqueezeGlow;
    }
}
