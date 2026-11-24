using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class LightTotalReflection_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxSunConsume", 3m),
        new BlockVar(9m, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, KazamiYukaKeywords.FierceSun];

    public LightTotalReflection_Yuka() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        ConsumeUpToMaxSunlight(base.Owner, DynamicVars["MaxSunConsume"].IntValue, play, out int actualSunlightSpent);
        await CreatureCmd.GainBlock(base.Owner.Creature, DynamicVars.Block, play, false);

        if (actualSunlightSpent > 0 &&
            base.Pile != null &&
            base.Pile.Type != PileType.Hand &&
            base.Pile.Type != PileType.Exhaust)
        {
            await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Bottom, null, false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxSunConsume"].UpgradeValueBy(-1m);
        DynamicVars.Block.UpgradeValueBy(3m);
    }

}
