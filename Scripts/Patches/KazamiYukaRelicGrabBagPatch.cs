using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace KazamiYuka.Scripts.Patches;

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
