using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.Create))]
public static class CombatRoomCreateTracePatch
{
    public static void Prefix()
    {
        GD.Print("[KazamiYuka][CombatTrace] NCombatRoom.Create entered.");
    }

    public static void Postfix(NCombatRoom? __result)
    {
        GD.Print($"[KazamiYuka][CombatTrace] NCombatRoom.Create completed. Valid={__result != null}.");
    }
}

[HarmonyPatch(typeof(NCombatRoom), "_Ready")]
public static class CombatRoomReadyTracePatch
{
    public static void Prefix()
    {
        GD.Print("[KazamiYuka][CombatTrace] NCombatRoom._Ready entered.");
    }

    public static void Postfix()
    {
        GD.Print("[KazamiYuka][CombatTrace] NCombatRoom._Ready completed.");
    }
}

[HarmonyPatch(typeof(NCombatRoom), "OnCombatSetUp")]
public static class CombatRoomSetupTracePatch
{
    public static void Prefix(CombatState state)
    {
        GD.Print($"[KazamiYuka][CombatTrace] NCombatRoom.OnCombatSetUp entered. Players={state?.Players?.Count ?? -1}.");
    }

    public static void Postfix()
    {
        GD.Print("[KazamiYuka][CombatTrace] NCombatRoom.OnCombatSetUp completed.");
    }
}

[HarmonyPatch(typeof(CombatRoom), "StartCombat")]
public static class CombatRoomStartTracePatch
{
    public static void Prefix(CombatRoom __instance)
    {
        GD.Print($"[KazamiYuka][CombatTrace] CombatRoom.StartCombat entered. Encounter={__instance.Encounter.Id.Entry}.");
    }

    public static void Postfix()
    {
        GD.Print("[KazamiYuka][CombatTrace] CombatRoom.StartCombat task completed.");
    }
}

[HarmonyPatch(typeof(CombatState), nameof(CombatState.CreateCreature))]
public static class CombatCreatureTracePatch
{
    public static void Prefix(MonsterModel monster, CombatSide side, string slot)
    {
        GD.Print($"[KazamiYuka][CombatTrace] CombatState.CreateCreature entered. Monster={monster?.Id.Entry ?? "<null>"} Side={side} Slot={slot}.");
    }

    public static void Postfix(Creature? __result)
    {
        GD.Print($"[KazamiYuka][CombatTrace] CombatState.CreateCreature completed. Valid={__result != null}.");
    }
}
