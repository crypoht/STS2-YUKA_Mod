using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.Powers;

namespace KazamiYuka.Patches;

[HarmonyPatch(typeof(NTargetManager), nameof(NTargetManager.StartTargeting), new Type[] { typeof(TargetType), typeof(Control), typeof(TargetMode), typeof(Func<bool>), typeof(Func<Node, bool>) })]
public class UITargetingVisualsPatch
{

    public static void Prefix(Control control, ref Func<Node, bool> nodeFilter)
    {
        var holder = control.GetParent() as MegaCrit.Sts2.Core.Nodes.Cards.Holders.NHandCardHolder;
        SunflowerManager.SetActiveTargetingCard(holder?.CardModel);
        if (holder != null && (holder.CardModel is ICustomTargetValidator || SqueezeManager.IsSqueezed(holder.CardModel)))
        {
            var originalFilter = nodeFilter;
            nodeFilter = (node) =>
            {
                if (node is NCreature nCreature &&
                    TyrantFormPower.AllowsSunflowerTarget(holder.CardModel, nCreature.Entity))
                {
                    return true;
                }

                if (node is NCreature customCreature)
                {
                    if (holder.CardModel is ICustomTargetValidator validator)
                    {
                        return (originalFilter == null || originalFilter(node)) &&
                               validator.IsValidCustomTarget(customCreature.Entity);
                    }

                    return SqueezeManager.IsValidOriginalOrSunflowerTarget(
                        holder.CardModel,
                        customCreature.Entity);
                }

                return false;
            };
        }
        else
        {
            var originalFilter = nodeFilter;
            nodeFilter = node =>
            {
                if (node is NCreature nCreature &&
                    TyrantFormPower.AllowsSunflowerTarget(holder?.CardModel, nCreature.Entity))
                {
                    return true;
                }

                return originalFilter?.Invoke(node) ?? true;
            };
        }
    }
}
