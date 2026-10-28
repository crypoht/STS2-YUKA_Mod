using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class NeverMercy_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("SunlightY", 0m),
        SunflowerDamageVar(0m),
        new DynamicVar("HitCount", 0m),
        new DynamicVar("HitBonus", 0m),
        new DynamicVar("SunlightDivisor", 3m)
    ];

    public NeverMercy_Yuka()
        : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    protected override bool HasEnergyCostX => true;

    protected override bool IsPlayable
    {
        get
        {
            RefreshDynamicPreview(false);
            return base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        if (cardPlay.IsFirstInSeries)
        {
            RefreshDynamicPreview(true);
        }
        int hitCount = DynamicVars["HitCount"].IntValue;
        decimal damage = DynamicVars["SunflowerDamage"].BaseValue;
        if (!IsFreeFromReceipt(cardPlay))
        {
            SunlightManager.SpendAllSunlight(base.Owner);
        }

        if (hitCount <= 0 || damage <= 0m)
        {
            return;
        }

        foreach (Creature sunflower in SunflowerManager.GetSunflowers(base.Owner).Where(flower => flower.IsAlive).ToList())
        {
            Creature? target = cardPlay.Target;
            for (int index = 0; index < hitCount; index++)
            {
                target = GetSunflowerDamageTarget(target);
                if (target == null)
                {
                    break;
                }

                await CreatureCmd.Damage(
                    choiceContext,
                    target,
                    damage,
                    ValueProp.Move,
                    sunflower,
                    this,
                    cardPlay
                );
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HitBonus"].UpgradeValueBy(1m);
    }

    private void RefreshDynamicPreview(bool useCapturedEnergy)
    {
        if (IsCanonical || HasBeenRemovedFromState)
        {
            return;
        }

        Player? owner = base.Owner;
        int rawSunlight = owner == null ? 0 : SunlightManager.GetSunlight(owner);
        int sunlightY = ResolveYValue(rawSunlight);
        DynamicVars["SunlightY"].BaseValue = sunlightY;
        DynamicVars["SunflowerDamage"].BaseValue = Math.Floor(sunlightY / DynamicVars["SunlightDivisor"].BaseValue);
        int energyX = useCapturedEnergy
            ? ResolveEnergyXValue()
            : ResolveEnergyXPreview();
        DynamicVars["HitCount"].BaseValue = energyX + DynamicVars["HitBonus"].BaseValue;
    }

    private int ResolveYValue(int rawSunlight)
    {
        return base.CombatState == null
            ? rawSunlight
            : Hook.ModifyXValue(base.CombatState, this, rawSunlight);
    }

    private int ResolveEnergyXPreview()
    {
        int rawEnergy = base.Owner?.PlayerCombatState == null
            ? 0
            : base.EnergyCost.GetAmountToSpend();
        return base.CombatState == null
            ? rawEnergy
            : Hook.ModifyXValue(base.CombatState, this, rawEnergy);
    }
}
