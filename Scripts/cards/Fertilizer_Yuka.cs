using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(TokenCardPool))]
public sealed class Fertilizer_Yuka : KazamiYukaCard, ICustomTargetValidator
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    public override IEnumerable<MegaCrit.Sts2.Core.Entities.Cards.CardKeyword> CanonicalKeywords => [
        MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Exhaust
    ];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxHpGain", 3m),
        new CardsVar(1)
    ];

    public Fertilizer_Yuka()
        : base(0, CardType.Skill, CardRarity.Token, KazamiYukaTargetTypes.Sunflower, true)
    {
    }

    public bool IsValidCustomTarget(Creature target) => target.Monster is KazamiYukaSunflower;

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Target?.Monster is not KazamiYukaSunflower)
        {
            return;
        }

        await SunflowerManager.GainMaxHpSingle(context, play.Target, DynamicVars["MaxHpGain"].BaseValue);
        await CardPileCmd.Draw(context, DynamicVars.Cards.BaseValue, base.Owner, false);
    }

    protected override void OnUpgrade() => DynamicVars["MaxHpGain"].UpgradeValueBy(1m);
}
