using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using KazamiYuka.Scripts;
using STS2RitsuLib.Content;

namespace KazamiYuka.Scripts.Keywords;

[RegisterOwnedCardKeyword(nameof(Grow))]
[RegisterOwnedCardKeyword(nameof(FierceSun))] 
public class KazamiYukaKeywords
{
    public static readonly string Grow = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Grow));
    
    public static readonly string FierceSun = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(FierceSun));
}