using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Keywords;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using KazamiYuka.Scripts.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class Pollination_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4m, ValueProp.Move),
        new PowerVar<ParasitePower>(1m),
        new DynamicVar("MaxSunlight", 6m),
        new CalculationBaseVar(1m),
        new CalculationExtraVar(1m),
        new CalculatedVar("HitCount").WithMultiplier((CardModel card, Creature? _) =>
        {
            bool isFree = IsEnergyCostTemporarilyFree(card);
            int availableSunlight = isFree
                ? (int)card.DynamicVars["MaxSunlight"].BaseValue
                : (card.Owner != null ? Math.Min(SunlightManager.GetSunlight(card.Owner), (int)card.DynamicVars["MaxSunlight"].BaseValue) : 0);
            return availableSunlight / 3;
        })
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<ParasitePower>(null)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [KazamiYukaKeywords.FierceSun];

    public Pollination_Yuka()
        : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        int consumedSunlight = ConsumeUpToMaxSunlight(
            base.Owner,
            DynamicVars["MaxSunlight"].IntValue,
            cardPlay
        );
        int hitCount = 1 + consumedSunlight / 2;

        for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash", null, null)
                .Execute(choiceContext);

            await PowerCmd.Apply<ParasitePower>(
                choiceContext,
                cardPlay.Target,
                DynamicVars["ParasitePower"].BaseValue,
                base.Owner.Creature,
                this,
                false
            );
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
