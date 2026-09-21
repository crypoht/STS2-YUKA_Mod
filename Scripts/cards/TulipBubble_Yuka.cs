using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class TulipBubble_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxHpGain", 15m),
        new PowerVar<BubblePower>(25m)
    ];

    public TulipBubble_Yuka()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SunflowerManager.GainMaxHpAll(choiceContext, base.Owner, DynamicVars["MaxHpGain"].BaseValue);

        foreach (var sunflower in SunflowerManager.GetSunflowers(base.Owner).Where(flower => flower.IsAlive).ToList())
        {
            await PowerCmd.Apply<BubblePower>(
                choiceContext,
                sunflower,
                DynamicVars["BubblePower"].BaseValue,
                base.Owner.Creature,
                this,
                false
            );
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BubblePower"].UpgradeValueBy(-5m);
    }
}
