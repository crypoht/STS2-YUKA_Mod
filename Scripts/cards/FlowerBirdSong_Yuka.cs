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
public sealed class FlowerBirdSong_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxSunlight", 8m),
        new SunlightCostVar(4m)
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, KazamiYukaKeywords.FierceSun];
    public FlowerBirdSong_Yuka() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (!TryPayFixedSunlight(base.Owner, DynamicVars["SunlightCost"].IntValue, play))
        {
            return;
        }

        int sunlight = ConsumeUpToMaxSunlight(base.Owner, DynamicVars["MaxSunlight"].IntValue, play);
        SunlightManager.GainSunlight(base.Owner, sunlight * 3);
        await Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SunlightCost"].UpgradeValueBy(-1m);
    }
}
