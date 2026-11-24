using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using KazamiYuka.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts.Cards;

namespace KazamiYuka.Scripts;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class Photosynthesis_Yuka : KazamiYukaCard
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("OldPerSun", 5m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public Photosynthesis_Yuka() : base(energyCost, type, rarity, TargetType.Self, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int sunlight = SunlightManager.GetSunlight(base.Owner);
        if (sunlight > 0)
        {
            await PowerCmd.Apply<OldPower>(
                choiceContext,
                base.Owner.Creature,
                sunlight * DynamicVars["OldPerSun"].BaseValue,
                base.Owner.Creature,
                this,
                false
            );
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
