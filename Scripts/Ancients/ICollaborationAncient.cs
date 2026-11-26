using MegaCrit.Sts2.Core.Modding;

namespace KazamiYuka.Scripts.Ancients;

public interface ICollaborationAncient
{
    IReadOnlyList<string> RequiredModIds { get; }
}

internal static class AncientCollaboration
{
    internal static bool AreModsLoaded(IReadOnlyList<string> requiredModIds)
    {
        var loadedModIds = ModManager.GetLoadedMods()
            .Select(mod => mod.manifest?.id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return requiredModIds.All(loadedModIds.Contains);
    }
}
