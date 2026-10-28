using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.CardTags;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts.Cards;
using KazamiYuka.Scripts.DynamicVars;

namespace KazamiYuka.Scripts;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class Photosynthesis_Yuka : KazamiYukaCard, ICustomTargetValidator
{
    protected override HashSet<CardTag> CanonicalTags => [SunflowerCardTags.Sunflower];

    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxHpGain", 8m),
        new SunlightCostVar(2m)
    ];

    public Photosynthesis_Yuka() : base(energyCost, type, rarity, KazamiYukaTargetTypes.Sunflower, shouldShowInCardLibrary)
    {
    }

    public bool IsValidCustomTarget(Creature target)
    {
        return target.Monster is KazamiYukaSunflower;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;

        if (target != null && target.Monster is KazamiYukaSunflower specificFlower)
        {
            if (!TryPayFixedSunlight(base.Owner, DynamicVars["SunlightCost"].IntValue, cardPlay))
            {
                return;
            }

            await SunflowerManager.GainMaxHpSingle(
                choiceContext,
                specificFlower.Creature,
                DynamicVars["MaxHpGain"].BaseValue
            );
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxHpGain"].UpgradeValueBy(3m);
    }
}
