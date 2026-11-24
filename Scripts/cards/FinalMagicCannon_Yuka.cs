using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class FinalMagicCannon_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar("Damage", 0m, ValueProp.Move),
        new DynamicVar("HitCount", 0m),
        new FinalMagicCannonTotalDamageVar(),
        new DynamicVar("LockedSunlight", 0m),
        new DynamicVar("SunlightDivisor", 3m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [KazamiYukaKeywords.FierceSunInfinite];

    public FinalMagicCannon_Yuka() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies, true)
    {
    }

    protected override bool IsPlayable
    {
        get
        {
            RefreshDynamicPreview();
            return base.IsPlayable;
        }
    }

    private void RefreshDynamicPreview()
    {
        if (IsCanonical || HasBeenRemovedFromState)
        {
            return;
        }

        Player? owner = base.Owner;
        DynamicVars["LockedSunlight"].BaseValue = owner == null
            ? 0m
            : SunlightManager.GetFierceSunlight(owner);
        int damage = (int)Math.Floor(
            DynamicVars["LockedSunlight"].BaseValue / DynamicVars["SunlightDivisor"].BaseValue);
        DynamicVars["Damage"].BaseValue = damage;
        DynamicVars["HitCount"].BaseValue = damage;
        DynamicVars["TotalDamage"].BaseValue =
            DynamicVars["Damage"].BaseValue * DynamicVars["HitCount"].BaseValue;
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (base.CombatState == null)
        {
            return;
        }

        Player owner = base.Owner;
        int sunlightToConsume = play.IsFirstInSeries
            ? SunlightManager.GetFierceSunlight(owner)
            : DynamicVars["LockedSunlight"].IntValue;
        if (sunlightToConsume <= 0)
        {
            return;
        }

        int hitCount = (int)Math.Floor(sunlightToConsume / DynamicVars["SunlightDivisor"].BaseValue);
        DynamicVars["LockedSunlight"].BaseValue = sunlightToConsume;
        DynamicVars["HitCount"].BaseValue = hitCount;
        int damage = (int)Math.Floor(
            sunlightToConsume / DynamicVars["SunlightDivisor"].BaseValue);
        DynamicVars["Damage"].BaseValue = damage;
        DynamicVars["TotalDamage"].BaseValue =
            DynamicVars["Damage"].BaseValue * DynamicVars["HitCount"].BaseValue;

        if (!IsFreeFromReceipt(play))
        {
            SunlightManager.SpendAllSunlight(owner);
        }

        if (hitCount <= 0)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars["Damage"].BaseValue)
            .WithHitCount(hitCount)
            .FromCard(this, play)
            .TargetingAllOpponents(base.CombatState)
            .Execute(context);
    }

    protected override void OnUpgrade()
    {
        base.AddKeyword(CardKeyword.Retain);
    }

    private sealed class FinalMagicCannonTotalDamageVar : DynamicVar
    {
        public FinalMagicCannonTotalDamageVar() : base("TotalDamage", 0m)
        {
        }

        public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
        {
            decimal damagePerHit = card.DynamicVars["Damage"].BaseValue;
            decimal hitCount = card.DynamicVars["HitCount"].BaseValue;
            decimal rawTotal = damagePerHit * hitCount;

            if (!runGlobalHooks || damagePerHit <= 0m || hitCount <= 0m || card.Owner?.Creature == null)
            {
                BaseValue = rawTotal;
                return;
            }

            var combatState = card.CombatState ?? card.Owner.Creature.CombatState;
            if (combatState == null)
            {
                BaseValue = rawTotal;
                return;
            }

            BaseValue = rawTotal;

            decimal modifiedDamage = Hook.ModifyDamage(
                card.Owner.RunState,
                combatState,
                target,
                card.Owner.Creature,
                damagePerHit,
                ValueProp.Move,
                card,
                null,
                ModifyDamageHookType.All,
                previewMode,
                out IEnumerable<AbstractModel> _);

            PreviewValue = Math.Max(0m, (int)modifiedDamage) * hitCount;
        }
    }
}
