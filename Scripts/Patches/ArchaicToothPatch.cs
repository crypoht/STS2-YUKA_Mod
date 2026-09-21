using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(ArchaicTooth), "get_TranscendenceUpgrades")]
public static class ArchaicToothPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref Dictionary<ModelId, CardModel> __result)
    {
        __result[ModelDb.Card<Cards.Sunshine_Yuka>().Id] = ModelDb.Card<Cards.Fantasy_Yuka>();
    }
}
