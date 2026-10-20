using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class HighIllumination_Yuka : KazamiYukaCard
{
    protected override HashSet<CardTag> CanonicalTags => [SunflowerCardTags.Sunflower];

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DamageAmount", 5m)
    ];

    public HighIllumination_Yuka() : base(1, CardType.Skill, CardRarity.Common, TargetType.None, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        if (player == null) return;

        decimal buffAmount = DynamicVars["DamageAmount"].BaseValue;
        foreach (var sunflower in SunflowerManager.GetSunflowers(player))
        {
            await PowerCmd.Apply<HighIlluminationPower>(
                choiceContext,
                sunflower,
                buffAmount,
                base.Owner.Creature,
                this,
                false
            );
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars["DamageAmount"].UpgradeValueBy(3m);
    }
}
