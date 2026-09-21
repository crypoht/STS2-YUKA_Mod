using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Interfaces;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class Breeze_Yuka : KazamiYukaCard, IVariableSunlightCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxSunConsume", 4m),
        new DynamicVar("HpUp", 2m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [KazamiYukaKeywords.FierceSun];

    public Breeze_Yuka() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    public int GetMaxSunlightConsume()
    {
        return (int)DynamicVars["MaxSunConsume"].BaseValue;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner;
        if (player == null) return;

        int consumedSunlight = ConsumeUpToMaxSunlight(player, GetMaxSunlightConsume(), cardPlay);
        if (consumedSunlight > 0)
        {
            await SunflowerManager.GainMaxHpAll(choiceContext, player, consumedSunlight*DynamicVars["HpUp"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxSunConsume"].UpgradeValueBy(1m);
    }
}
