using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class SunlightDiffusion_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new DynamicVar("Damage", 4m),
        new CalculatedVar("HitCount").WithMultiplier(GetSunlightGainedThisTurn)
    ];

    public SunlightDiffusion_Yuka() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true) { }

    private static decimal GetSunlightGainedThisTurn(
        MegaCrit.Sts2.Core.Models.CardModel card,
        Creature? _)
    {
        return card.Owner == null
            ? 0m
            : SunlightManager.GetSunlightGainedThisTurn(card.Owner);
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (base.Owner != null && side == base.Owner.Creature.Side)
        {
            SunlightManager.ResetSunlightGainedThisTurn(base.Owner);
        }

        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Target == null)
        {
            return;
        }

        int hitCount = (int)((CalculatedVar)DynamicVars["HitCount"]).Calculate(play.Target);
        if (hitCount <= 0)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars["Damage"].BaseValue)
            .WithHitCount(hitCount)
            .FromCard(this, play)
            .Targeting(play.Target)
            .Execute(context);
    }

     protected override void OnUpgrade()
    {
        DynamicVars["Damage"].UpgradeValueBy(1m);
    }
}
