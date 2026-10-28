using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(RelicGrabBag), nameof(RelicGrabBag.Populate), typeof(IEnumerable<RelicModel>), typeof(Rng))]
public static class KazamiYukaTreasureRelicGrabBagPatch
{
    public static void Prefix(RelicGrabBag __instance, ref IEnumerable<RelicModel> relics)
    {
        var runState = RunManager.Instance?.DebugOnlyGetState();
        if (runState == null || __instance != runState.SharedRelicGrabBag)
        {
            return;
        }

        var player = runState.Players.FirstOrDefault(candidate => candidate.Character is KazamiYukaCharacter);
        if (player == null)
        {
            return;
        }

        var exclusiveRelics = player.Character.RelicPool.GetUnlockedRelics(player.UnlockState)
            .Where(relic => relic.Rarity is RelicRarity.Common or RelicRarity.Uncommon or RelicRarity.Rare)
            .ToList();
        relics = relics.Concat(exclusiveRelics).DistinctBy(relic => relic.Id).ToList();
        Entry.Logger.Info($"[RelicPool] Added {exclusiveRelics.Count} Yuka relics to the treasure pool: {string.Join(", ", exclusiveRelics.Select(relic => relic.Id))}");
    }
}

[HarmonyPatch(typeof(RelicGrabBag), nameof(RelicGrabBag.Populate), typeof(Player), typeof(Rng))]
public static class KazamiYukaPlayerRelicGrabBagPatch
{
    public static void Postfix(RelicGrabBag __instance, Player player)
    {
        if (player.Character is not KazamiYukaCharacter)
        {
            return;
        }

        var exclusiveRelics = player.Character.RelicPool.GetUnlockedRelics(player.UnlockState)
            .Where(relic => relic.Rarity is RelicRarity.Common or RelicRarity.Uncommon or RelicRarity.Rare)
            .ToList();
        var presentRelics = exclusiveRelics.Where(__instance.Contains).ToList();
        Entry.Logger.Info($"[RelicPool] Player {player.NetId}: {presentRelics.Count}/{exclusiveRelics.Count} Yuka relics in reward/shop pool: {string.Join(", ", presentRelics.Select(relic => relic.Id))}");
    }
}
