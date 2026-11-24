using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS2RitsuLib.Keywords;
using KazamiYuka.Scripts.Keywords;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class SunlightRepeater_Yuka : KazamiYukaCard
{
    public SunlightRepeater_Yuka() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [KazamiYukaKeywords.FierceSun];

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            DynamicVar[] array = new DynamicVar[4];
            array[0] = new CalculationBaseVar(12m);      
            array[1] = new ExtraDamageVar(3m);          
            array[2] = new DynamicVar("MaxSunConsume", 10m); 
            
            array[3] = new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, MegaCrit.Sts2.Core.Entities.Creatures.Creature? _) => 
            {
                int maxSunLimit = (int)card.DynamicVars["MaxSunConsume"].BaseValue;

                if (IsEnergyCostTemporarilyFree(card)) return maxSunLimit;

                return card.Owner != null ? Math.Min(SunlightManager.GetFierceSunlight(card.Owner), maxSunLimit) : 0;
            });

            return array;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
        if (player == null) return;
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        int maxSunLimit = (int)DynamicVars["MaxSunConsume"].BaseValue;

        int actualSpent = base.ConsumeUpToMaxSunlight(player, maxSunLimit, cardPlay);

        decimal finalDamage = DynamicVars.CalculationBase.BaseValue +
            DynamicVars.ExtraDamage.BaseValue * actualSpent;

        await DamageCmd.Attack(finalDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_starry_impact", null, null)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxSunConsume"].UpgradeValueBy(5m);
    }
}
