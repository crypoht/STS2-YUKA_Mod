using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using KazamiYuka.Scripts;
using STS2RitsuLib.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace KazamiYuka.Scripts.Keywords;

[RegisterOwnedCardKeyword(nameof(Grow))]
[RegisterOwnedCardKeyword(nameof(FierceSun))] 
[RegisterOwnedCardKeyword(nameof(FierceSunInfinite))]
public class KazamiYukaKeywords
{
    public static readonly CardKeyword Grow = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Grow)).GetModCardKeyword();
    
    public static readonly CardKeyword FierceSun = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(FierceSun)).GetModCardKeyword();

    public static readonly CardKeyword FierceSunInfinite = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(FierceSunInfinite)).GetModCardKeyword();
}
