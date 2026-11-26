using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;
using STS2RitsuLib.Data;
using STS2RitsuLib.RunData;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;

namespace KazamiYuka.Scripts;

public sealed class YukaSettings
{
    public bool EnableSpecialAncients { get; set; } = true;
}

public sealed class YukaRunSettings
{
    public bool EnableSpecialAncients { get; set; } = true;
}

internal static class YukaSettingsRegistration
{
    private const string SettingsKey = "settings";
    private static RunSavedData<YukaRunSettings> _runSettings = null!;

    internal static void Register()
    {
        using (RitsuLibFramework.BeginModDataRegistration(Entry.ModId))
        {
            RitsuLibFramework.GetDataStore(Entry.ModId).Register(
                key: SettingsKey,
                fileName: "settings.json",
                scope: SaveScope.Global,
                syncToCloud: false,
                defaultFactory: () => new YukaSettings(),
                autoCreateIfMissing: true);

            _runSettings = RitsuLibFramework.GetRunSavedDataStore(Entry.ModId).Register(
                key: "run_settings",
                defaultFactory: () => new YukaRunSettings(),
                options: new RunSavedDataOptions { WritePolicy = RunSavedDataWritePolicy.WhenSet });
        }

        RitsuLibFramework.RegisterModSettings(Entry.ModId, page => page
            .AddSection("ancient_settings", section => section
                .WithTitle(SettingsText("ANCIENT_SETTINGS.title"))
                .AddToggle(
                    "enable_special_ancients",
                    SettingsText("ENABLE_SPECIAL_ANCIENTS.title"),
                    new ModSettingsValueBinding<YukaSettings, bool>(
                        Entry.ModId, SettingsKey, SaveScope.Global,
                        settings => settings.EnableSpecialAncients,
                        (settings, value) => settings.EnableSpecialAncients = value),
                    description: SettingsText("ENABLE_SPECIAL_ANCIENTS.description"))));

        RitsuLibFramework.SubscribeLifecycle<RunSavedDataLobbyStagingEvent>(OnLobbyStaging, replayCurrentState: false);
        RitsuLibFramework.SubscribeLifecycle<RunSavedDataPreparingEvent>(OnRunPreparing, replayCurrentState: false);
    }

    internal static bool AreSpecialAncientsEnabled(RunState runState) =>
        !_runSettings.TryGet(runState, out var settings) || settings.EnableSpecialAncients;

    private static void OnLobbyStaging(RunSavedDataLobbyStagingEvent evt)
    {
        if (evt.Reason == RunSavedDataLobbyStagingReason.Committing && (evt.IsHost || !evt.IsMultiplayer))
            _runSettings.Lobby.Set(evt.Lobby, CaptureLocalSettings());
    }

    private static void OnRunPreparing(RunSavedDataPreparingEvent evt)
    {
        if (!evt.IsMultiplayer && !_runSettings.TryGet(evt.RunState, out _))
            _runSettings.Set(evt.RunState, CaptureLocalSettings());
    }

    private static YukaRunSettings CaptureLocalSettings() => new()
    {
        EnableSpecialAncients = RitsuLibFramework.GetDataStore(Entry.ModId)
            .Get<YukaSettings>(SettingsKey).EnableSpecialAncients
    };

    private static ModSettingsText SettingsText(string key)
    {
        var locKey = "KAZAMI_YUKA." + key;
        return ModSettingsText.LocString("settings_ui", locKey, locKey);
    }
}
