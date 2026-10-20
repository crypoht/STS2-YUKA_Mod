using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.AddCreature))]
internal static class SunflowerPlacementPatch
{
    private static void Postfix(Creature creature)
    {
        if (creature.Monster is not KazamiYukaSunflower ||
            creature.PetOwner == null ||
            CombatManager.Instance is not { IsInProgress: true, IsOverOrEnding: false })
        {
            return;
        }

        GD.Print("[KazamiYuka][CombatTrace] SunflowerPlacementPatch arranged a sunflower.");
        SunflowerManager.ArrangeSunflowersDeferred(creature.PetOwner);
    }
}

[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.RemoveCreatureNode))]
internal static class SunflowerRemovalPlacementPatch
{
    private static void Postfix(NCreature node)
    {
        if (node.Entity.Monster is KazamiYukaSunflower)
        {
            SunflowerManager.ArrangeAllSunflowersDeferred();
        }
    }
}
