using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Combat.CardTargeting;

namespace KazamiYuka.Scripts;

public static class KazamiYukaTargetTypes
{
    public static TargetType Sunflower { get; } =
        CustomTargetType.RegisterSingleTargetType(
            Entry.ModId,
            "sunflower",
            target => target.IsAlive && target.Monster is KazamiYukaSunflower);
}
