using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
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
        if (holder?.CardModel == null)
        {
            return;
        }

        if (holder != null && (holder.CardModel is ICustomTargetValidator || SqueezeManager.IsSqueezed(holder.CardModel)))
        {
            var originalFilter = nodeFilter;
            nodeFilter = (node) =>
            {
                if (SqueezeManager.IsSqueezed(holder.CardModel))
                {
                    return node switch
                    {
                        NCreature creature => SqueezeManager.IsValidOriginalOrSunflowerTarget(
                            holder.CardModel, creature.Entity),
                        NMultiplayerPlayerState playerState => SqueezeManager.IsValidOriginalOrSunflowerTarget(
                            holder.CardModel, playerState.Player.Creature),
                        _ => false
                    };
                }

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

[HarmonyPatch(typeof(NTargetManager), nameof(NTargetManager.StartTargeting), new Type[] { typeof(TargetType), typeof(Vector2), typeof(TargetMode), typeof(Func<bool>), typeof(Func<Node, bool>) })]
public static class NonCardTargetingStatePatch
{
    public static void Prefix()
    {
        SunflowerManager.SetActiveTargetingCard(null);
    }
}
