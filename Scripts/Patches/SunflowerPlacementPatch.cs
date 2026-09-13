using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using KazamiYuka.Scripts;

namespace KazamiYuka.Patches;

[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.AddCreature))]
public static class SunflowerPlacementPatch
{
    [HarmonyPostfix]
    public static void Postfix(Creature creature)
    {
        if (creature.Monster is not KazamiYukaSunflower || creature.PetOwner is not { } owner)
        {
            return;
        }

        SunflowerManager.ArrangeSunflowers(owner);
    }
}
