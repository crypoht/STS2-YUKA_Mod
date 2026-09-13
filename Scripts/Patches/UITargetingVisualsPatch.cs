using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using KazamiYuka.Scripts;

namespace KazamiYuka.Patches;

[HarmonyPatch(typeof(NTargetManager), nameof(NTargetManager.StartTargeting), new Type[] { typeof(TargetType), typeof(Control), typeof(TargetMode), typeof(Func<bool>), typeof(Func<Node, bool>) })]
public class UITargetingVisualsPatch
{

    public static void Prefix(Control control, ref Func<Node, bool> nodeFilter)
    {
        var holder = control.GetParent() as MegaCrit.Sts2.Core.Nodes.Cards.Holders.NHandCardHolder;
        if (holder != null && holder.CardModel is ICustomTargetValidator validator)
        {
            var originalFilter = nodeFilter;
            nodeFilter = (node) =>
            {
                if (originalFilter != null && !originalFilter(node)) return false;
                if (node is NCreature nCreature) return validator.IsValidCustomTarget(nCreature.Entity);
                return false;
            };
        }
    }
}