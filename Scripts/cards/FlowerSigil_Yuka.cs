using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.DynamicVars;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class FlowerSigil_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxSunlight", 4m),
        new DynamicVar("Summon", 6m)
    ];

    public FlowerSigil_Yuka() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [KazamiYukaKeywords.FierceSun, KazamiYukaKeywords.Grow];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        int sunlight = ConsumeUpToMaxSunlight(base.Owner, DynamicVars["MaxSunlight"].IntValue, play);
        for (int index = 0; index <= sunlight / 2; index++)
        {
            await SunflowerManager.Summon(base.Owner, DynamicVars["Summon"].BaseValue);
        }
    }

    protected override void OnUpgrade() => DynamicVars["Summon"].UpgradeValueBy(2m);
}
