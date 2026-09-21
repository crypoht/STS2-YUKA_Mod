using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class RapidSowing_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Summon", 1m)];
    public RapidSowing_Yuka() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self, true) { }

    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, KazamiYukaKeywords.Grow];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        while (SunflowerManager.GetSunflowers(base.Owner).Count < 5)
        {
            await SunflowerManager.Summon(base.Owner, DynamicVars["Summon"].BaseValue, grantDegraded: true);
        }
    }

    protected override void OnUpgrade() => base.AddKeyword(CardKeyword.Innate);
}
