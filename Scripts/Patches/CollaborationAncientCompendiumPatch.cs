using System.Reflection.Emit;
using HarmonyLib;
using KazamiYuka.Scripts.Ancients;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.RelicCollection;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(NRelicCollectionCategory), nameof(NRelicCollectionCategory.LoadRelics))]
internal static class CollaborationAncientCompendiumPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var ancientType = new[] { typeof(AncientEventModel) };
        var matcher = new CodeMatcher(instructions).MatchEndForward(
            new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(ModelDb), nameof(ModelDb.AllSharedAncients))),
            new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Enumerable), nameof(Enumerable.Concat), generics: ancientType)),
            new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Enumerable), nameof(Enumerable.Distinct),
                [typeof(IEnumerable<AncientEventModel>)], ancientType)),
            new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Enumerable), nameof(Enumerable.ToList), generics: ancientType)));

        if (matcher.IsInvalid)
        {
            Log.Warn("[KazamiYuka] Could not find the Ancient list in the relic compendium.");
            return matcher.InstructionEnumeration();
        }

        matcher.Advance(1).Insert(CodeInstruction.Call(typeof(CollaborationAncientCompendiumPatch), nameof(FilterAncients)));
        return matcher.InstructionEnumeration();
    }

    private static List<AncientEventModel> FilterAncients(List<AncientEventModel> ancients)
    {
        ancients.RemoveAll(ancient => ancient is ICollaborationAncient collaboration
            && !AncientCollaboration.AreModsLoaded(collaboration.RequiredModIds));
        return ancients;
    }
}
