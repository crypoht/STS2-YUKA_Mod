using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.CardTags;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;

namespace KazamiYuka.Scripts;

[RegisterOwnedCardTag(nameof(Sunflower))]
public static class SunflowerCardTags
{
    public static readonly CardTag Sunflower =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(Sunflower)).GetModCardTag();

    public static bool IsSunflowerCard(CardModel? card) =>
        card != null &&
        (card.Tags.Contains(Sunflower) || card.TargetType == KazamiYukaTargetTypes.Sunflower);
}
