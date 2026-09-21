using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.AddCreature))]
internal static class SunflowerPlacementPatch
{
    private static void Postfix(Creature creature)
    {
        if (creature.Monster is not KazamiYukaSunflower || creature.PetOwner == null)
        {
            return;
        }

        SunflowerManager.ArrangeSunflowersDeferred(creature.PetOwner);
    }
}
