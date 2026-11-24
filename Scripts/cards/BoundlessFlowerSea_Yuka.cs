using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.DynamicVars;
using KazamiYuka.Scripts.Keywords;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class BoundlessFlowerSea_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<BoundlessFlowerSeaPower>(6m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [KazamiYukaKeywords.Grow];

    public BoundlessFlowerSea_Yuka()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        await PowerCmd.Apply<BoundlessFlowerSeaPower>(
            choiceContext,
            base.Owner.Creature,
            DynamicVars["BoundlessFlowerSeaPower"].BaseValue,
            base.Owner.Creature,
            this,
            false
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BoundlessFlowerSeaPower"].UpgradeValueBy(3m);
    }
}
