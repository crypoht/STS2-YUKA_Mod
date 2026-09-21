using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts.DynamicVars;
using KazamiYuka.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class FlowerShooting_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(75m),
        new ExtraDamageVar(-15m),
        new DynamicVar("MaxSunConsume", 4m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) =>
        {
            int maxSunConsume = card.DynamicVars["MaxSunConsume"].IntValue;
            if (IsEnergyCostTemporarilyFree(card))
            {
                return maxSunConsume;
            }

            return card.Owner != null ? Math.Min(SunlightManager.GetFierceSunlight(card.Owner), maxSunConsume) : 0;
        })
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, KazamiYukaKeywords.FierceSun];

    public FlowerShooting_Yuka() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true) { }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Target == null) return;
        ConsumeUpToMaxSunlight(base.Owner, DynamicVars["MaxSunConsume"].IntValue, play);
        decimal damage = DynamicVars.CalculatedDamage.PreviewValue;
        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .Execute(context);
    }

    protected override void OnUpgrade() => DynamicVars["MaxSunConsume"].UpgradeValueBy(-1m);
}
