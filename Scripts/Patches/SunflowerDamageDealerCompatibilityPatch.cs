using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterDamageReceived))]
public static class SunflowerDamageDealerCompatibilityPatch
{
    public static void Prefix(ref Creature? dealer, CardModel? cardSource)
    {
        if (dealer?.Monster is KazamiYukaSunflower)
        {
            dealer = cardSource?.Owner?.Creature ?? dealer.PetOwner?.Creature ?? dealer;
        }
    }
}

[HarmonyPatch(typeof(PenNib), nameof(PenNib.ModifyDamageMultiplicative))]
public static class SunflowerPenNibDamagePatch
{
    public static void Postfix(
        PenNib __instance,
        Creature? target,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay,
        ref decimal __result)
    {
        if (__result != 1m || !props.IsPoweredAttack() || cardSource == null ||
            dealer?.Monster is not KazamiYukaSunflower || cardSource.Owner != __instance.Owner)
        {
            return;
        }

        CardModel? attackToDouble = AccessTools.Field(typeof(PenNib), "_attackToDouble")?.GetValue(__instance) as CardModel;
        if (attackToDouble == cardSource ||
            (attackToDouble == null && cardSource.Pile?.Type != PileType.Play && __instance.AttacksPlayed == 9))
        {
            __result = 2m;
        }
    }
}
