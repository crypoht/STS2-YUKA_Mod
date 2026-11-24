using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public sealed class AncientVisitorTicket_Yuka : YukaAncientRelic
{
    private const int TravelLimit = 4;
    private int _timesUsed;

    public override bool IsUsedUp => _timesUsed >= TravelLimit;
    public override bool ShowCounter => !IsUsedUp;
    public override int DisplayAmount => TravelLimit - _timesUsed;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Rooms", TravelLimit)];

    [SavedProperty]
    public int TimesUsed
    {
        get => _timesUsed;
        set
        {
            AssertMutable();
            _timesUsed = value;
            DynamicVars["Rooms"].BaseValue = DisplayAmount;
            Status = IsUsedUp ? RelicStatus.Disabled : RelicStatus.Normal;
            InvokeDisplayAmountChanged();
        }
    }

    internal static AncientVisitorTicket_Yuka? FindAvailable(IRunState runState) => runState.Players
        .SelectMany(player => player.Relics.OfType<AncientVisitorTicket_Yuka>())
        .FirstOrDefault(relic => !relic.IsUsedUp && !relic.IsMelted);

    public override bool ShouldAllowFreeTravel() => !IsUsedUp;

    [HarmonyPatch(typeof(Hook), nameof(Hook.ShouldAllowFreeTravel))]
    private static class TeamTravelPatch
    {
        private static void Postfix(IRunState runState, ref bool __result)
        {
            __result = __result || FindAvailable(runState) != null;
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterRoomEntered))]
    private static class SpendTravelPatch
    {
        private static void Prefix(IRunState runState)
        {
            if (runState is not RunState state || state.CurrentRoomCount > 1 || state.VisitedMapCoords.Count <= 1)
                return;

            MapPoint? previous = state.Map.GetPoint(state.VisitedMapCoords[^2]);
            MapPoint? current = state.CurrentMapPoint;
            if (previous == null || current == null || previous.Children.Contains(current))
                return;

            AncientVisitorTicket_Yuka? ticket = FindAvailable(state);
            if (ticket != null)
            {
                ticket.Flash();
                ticket.TimesUsed++;
            }
        }
    }
}
