using System.Reflection;
using Godot;
using HarmonyLib;
using KazamiYuka.Scripts.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Ancients;

[RegisterSharedAncient]
public sealed class KazamiYukaAncient_Yuka : ModAncientEventTemplate
{
    private const string KoishiModId = "Komeiji_Koishi";
    private const string MapIconAssetPath = "res://mods/KazamiYuka/images/ancient/ancient_icon.png";
    private const string RunHistoryIconAssetPath = "res://mods/KazamiYuka/images/ancient/icon_64.png";

    internal static bool IsCollaborationEnabled => ModManager.GetLoadedMods().Any(mod =>
        string.Equals(mod.manifest?.id, KoishiModId, StringComparison.OrdinalIgnoreCase));

    public override bool IsValidForAct(ActModel act) => IsCollaborationEnabled;

    public override EventAssetProfile AssetProfile => new(
        BackgroundScenePath: "res://mods/KazamiYuka/scenes/ancients/KazamiYukaAncient_Yuka.tscn");

    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile => new(
        MapIconPath: MapIconAssetPath,
        MapIconOutlinePath: MapIconAssetPath,
        RunHistoryIconPath: RunHistoryIconAssetPath,
        RunHistoryIconOutlinePath: RunHistoryIconAssetPath);

    public override Color DialogueColor => new("965325");

    private List<EventOption> FirstPool() =>
        [CreateModRelicOption<AncientYoukaiUmbrella_Yuka>(),
         CreateModRelicOption<SunflowerCrown_Yuka>(),
         CreateModRelicOption<SleepingCap_Yuka>()];

    private List<EventOption> SecondPool() =>
        [CreateModRelicOption<SowingBag_Yuka>(),
         CreateModRelicOption<KaleidoscopeBouquet_Yuka>(),
         CreateModRelicOption<DaisyRelic_Yuka>()];

    private List<EventOption> ThirdPool() =>
        [CreateModRelicOption<AncientVisitorTicket_Yuka>(),
         CreateModRelicOption<MagicCannonRelic_Yuka>(),
         CreateModRelicOption<FantasyFortuneSlip_Yuka>()];

    public override IEnumerable<EventOption> AllPossibleOptions =>
        FirstPool().Concat(SecondPool()).Concat(ThirdPool());

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
        GetOptionsFromThreePools(FirstPool(), SecondPool(), ThirdPool());
}

[HarmonyPatch]
internal static class KazamiYukaAncientCatalogPatch
{
    private const string RitsuLibContentHarmonyId = "com.ritsukage.sts2-RitsuLib.framework-content-registry";

    private static IEnumerable<MethodBase> TargetMethods() =>
        [AccessTools.PropertyGetter(typeof(ModelDb), nameof(ModelDb.AllSharedAncients)),
         AccessTools.PropertyGetter(typeof(ModelDb), nameof(ModelDb.AllAncients))];

    [HarmonyAfter(RitsuLibContentHarmonyId)]
    [HarmonyPriority(Priority.Last - 1)]
    private static void Postfix(ref IEnumerable<AncientEventModel> __result)
    {
        if (!KazamiYukaAncient_Yuka.IsCollaborationEnabled)
            __result = __result.Where(ancient => ancient is not KazamiYukaAncient_Yuka);
    }
}

[HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateRooms), typeof(RunState))]
internal static class KazamiYukaAncientPartyPatch
{
    private static void Prefix(RunState runState, List<AncientEventModel>? ____sharedAncientSubset)
    {
        if (runState.Players.Any(player => player.Character is KazamiYukaCharacter))
            ____sharedAncientSubset?.RemoveAll(ancient => ancient is KazamiYukaAncient_Yuka);
    }
}
