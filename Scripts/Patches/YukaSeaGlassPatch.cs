using System.Reflection;
using HarmonyLib;
using KazamiYuka.Scripts.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace KazamiYuka.Scripts.Patches;

internal static class YukaSeaGlassPatch
{
    private const string LocalizationPrefix = "SEA_GLASS.KAZAMI_YUKA_CHARACTER_KAZAMI_YUKA_CHARACTER";

    private static bool IsYukaSeaGlass(RelicModel relic) =>
        relic is SeaGlass seaGlass && seaGlass.CharacterId != null &&
        seaGlass.CharacterId == ModelDb.Character<KazamiYukaCharacter>().Id;

    [HarmonyPatch(typeof(SeaGlass), nameof(SeaGlass.AfterObtained))]
    private static class ObtainPatch
    {
        private static bool Prefix(SeaGlass __instance, ref Task __result)
        {
            if (!IsYukaSeaGlass(__instance))
                return true;

            __result = YukaRewardCardSelection.ChooseCards(__instance.Owner,
                new LocString("relics", LocalizationPrefix + ".selectionScreenPrompt"));
            return false;
        }
    }

    [HarmonyPatch]
    private static class DescriptionPatch
    {
        private static IEnumerable<MethodBase> TargetMethods() =>
            [AccessTools.PropertyGetter(typeof(RelicModel), nameof(RelicModel.DynamicDescription)),
             AccessTools.PropertyGetter(typeof(RelicModel), nameof(RelicModel.DynamicEventDescription))];

        private static void Postfix(RelicModel __instance, ref LocString __result)
        {
            if (IsYukaSeaGlass(__instance))
                __result = new LocString("relics", LocalizationPrefix + ".description");
        }
    }
}
