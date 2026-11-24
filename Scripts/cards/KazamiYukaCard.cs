using System;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.Patches;
using Godot;

namespace KazamiYuka.Scripts.Cards;

public abstract class KazamiYukaCard : ModCardTemplate
{
    protected MegaCrit.Sts2.Core.Entities.Creatures.Creature? GetLowestAliveEnemy() =>
        CombatState?.Enemies
            .Where(enemy => enemy.IsAlive)
            .OrderBy(enemy => enemy.CurrentHp)
            .FirstOrDefault();

    protected MegaCrit.Sts2.Core.Entities.Creatures.Creature? GetSunflowerDamageTarget(
        MegaCrit.Sts2.Core.Entities.Creatures.Creature? preferredTarget = null) =>
        preferredTarget?.IsAlive == true ? preferredTarget : GetLowestAliveEnemy();

    private const string CardPortraitDirectory = "res://mods/KazamiYuka/images/cards";
    private const string TemporaryCardPortraitPath = $"{CardPortraitDirectory}/Temp_Yuka.png";

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected CardAssetProfile DefaultCardAssetProfile => new(
        PortraitPath: ResolveCardPortraitPath()
    );

    protected static MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar SunflowerDamageVar(decimal baseValue) =>
        STS2RitsuLib.Cards.DynamicVars.ModCardVars.ComputedDamage(
            "SunflowerDamage",
            baseValue,
            (card, _) => card?.DynamicVars["SunflowerDamage"].BaseValue ?? baseValue,
            card => card.Owner == null
                ? null
                : System.Linq.Enumerable.FirstOrDefault(SunflowerManager.GetSunflowers(card.Owner)));

    private string ResolveCardPortraitPath()
    {
        string portraitPath = $"{CardPortraitDirectory}/{GetType().Name}.png";
        return ResourceLoader.Exists(portraitPath)
            ? portraitPath
            : TemporaryCardPortraitPath;
    }

    public bool IsSunlightFree 
    {
        get 
        {
            return IsEnergyCostTemporarilyFree(this);
        }
    }

    public static bool IsEnergyCostTemporarilyFree(CardModel card)
    {
        var energy = card.EnergyCost;
        return energy != null
            && !energy.CostsX
            && energy.GetWithModifiers(CostModifiers.All) == 0
            && (Patches.FreeCardTracking.IsMarked(card)
                || energy.GetWithModifiers(CostModifiers.None) > 0);
    }

    protected KazamiYukaCard(int cost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary) 
        : base(cost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override bool IsPlayable
    {
        get
        {
           if (!base.IsPlayable) return false;

            if (IsEnergyCostTemporarilyFree(this))
            {
                return true;
            }

            if (DynamicVars.TryGetValue("SunlightCost", out var sunCostVar))
            {
                var player = base.Owner as Player;
                if (player != null)
                {
                    int requiredSunlight = (int)sunCostVar.BaseValue;
                    if (!SunlightManager.HasEnoughSunlight(player, requiredSunlight))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    protected bool IsFreeFromReceipt(CardPlay cardPlay)
    {
        if (cardPlay == null || cardPlay.Card?.EnergyCost == null) return false;

        return cardPlay.IsAutoPlay
            || !cardPlay.IsFirstInSeries
            || (cardPlay.Resources.EnergySpent == 0
            && IsEnergyCostTemporarilyFree(cardPlay.Card));
    }

    protected bool TryPayFixedSunlight(Player player, int requiredAmount, CardPlay cardPlay)
    {
        if (IsFreeFromReceipt(cardPlay))
        {
            return true;
        }

        if (PhantasmagoriaofFlowerViewPayment.TryConsumeFixedSunlightPayment(this, requiredAmount))
        {
            return true;
        }

        return SunlightManager.TrySpendSunlight(player, requiredAmount);
    }

    protected int ConsumeUpToMaxSunlight(Player player, int maxLimit, CardPlay cardPlay)
        => ConsumeUpToMaxSunlight(player, maxLimit, cardPlay, out _);

    protected int ConsumeUpToMaxSunlight(Player player, int maxLimit, CardPlay cardPlay, out int actualSunlightSpent)
    {
        actualSunlightSpent = 0;
        int sunAvailable = SunlightManager.GetFierceSunlight(player);
        int sunToSpend = Math.Min(sunAvailable, maxLimit);

        if (IsFreeFromReceipt(cardPlay))
        {
            return maxLimit;
        }
        
        if (sunToSpend > 0 && SunlightManager.GetSunlight(player) > 0)
        {
            if (SunlightManager.TrySpendSunlight(player, sunToSpend))
            {
                actualSunlightSpent = sunToSpend;
            }
        }
        
        return sunToSpend;
    }
}
