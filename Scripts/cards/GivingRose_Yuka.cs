using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class GivingRose_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxSunlight", 4m),
        new DynamicVar("RepeatThreshold", 4m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust,
        KazamiYukaKeywords.FierceSun
    ];

    public GivingRose_Yuka()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Target?.Player == null)
        {
            return;
        }

        Player targetPlayer = cardPlay.Target.Player;
        int consumedSunlight = ConsumeUpToMaxSunlight(
            base.Owner,
            DynamicVars["MaxSunlight"].IntValue,
            cardPlay
        );

        if (consumedSunlight <= 0)
        {
            return;
        }

        int repeatCount = 1 + consumedSunlight / DynamicVars["RepeatThreshold"].IntValue;
        foreach (PowerModel power in targetPlayer.Creature.Powers
                     .Where(power => power.TypeForCurrentAmount == PowerType.Buff)
                     .ToList())
        {
            await PowerCmd.ModifyAmount(
                choiceContext,
                power,
                repeatCount,
                base.Owner.Creature,
                this,
                false
            );
        }

        if (base.Owner.RunState.Rng.CombatCardGeneration.NextFloat(1f) < 0.02f)
        {
            CardModel afterimage = base.CombatState!.CreateCard<Afterimage>(base.Owner);
            await CardPileCmd.AddGeneratedCardToCombat(
                afterimage,
                PileType.Hand,
                base.Owner,
                CardPilePosition.Bottom
            );
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxSunlight"].UpgradeValueBy(4m);
    }
}
