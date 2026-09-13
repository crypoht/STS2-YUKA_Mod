using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using KazamiYuka.Scripts.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class ViolentArt_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(21m),
        new ExtraDamageVar(4m),
        new DynamicVar("SunlightSpentCount", 0m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) =>
            card.DynamicVars["SunlightSpentCount"].BaseValue),
        new SunlightCostVar(1m),
    ];

    public ViolentArt_Yuka() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    public override void AfterCreated()
    {
        SunlightManager.SunlightSpent += OnSunlightSpent;
        base.AfterCreated();
    }

    private void OnSunlightSpent(Player player, int amount)
    {
        if (HasBeenRemovedFromState)
        {
            SunlightManager.SunlightSpent -= OnSunlightSpent;
            return;
        }

        if (base.Owner == player)
        {
            DynamicVars["SunlightSpentCount"].BaseValue += 1m;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner;
        if (player == null || cardPlay.Target == null)
        {
            return;
        }

        int sunlightCost = DynamicVars["SunlightCost"].IntValue;
        if (!TryPayFixedSunlight(player, sunlightCost, cardPlay))
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.CalculatedDamage.Calculate(cardPlay.Target))
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash", null, null)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(4m);
        DynamicVars.ExtraDamage.UpgradeValueBy(2m);
    }
}
