using MegaCrit.Sts2.Core.Entities.Creatures;

namespace KazamiYuka.Scripts;

public interface ICustomTargetValidator
{
    bool IsValidCustomTarget(Creature target);
}