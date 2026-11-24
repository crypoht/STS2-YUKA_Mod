using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using KazamiYuka.Scripts.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.ValueProps;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public class ViolentArt_Yuka : KazamiYukaCard
{
    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(21m),
        new ExtraDamageVar(4m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) =>
            ViolentArtState.GetSpendCount(((ViolentArt_Yuka)card).Owner)),
        new SunlightCostVar(2m),
    ];

    public ViolentArt_Yuka() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
        ViolentArtState.EnsureSubscribed();
    }

    public override Task BeforeCombatStart()
    {
        ViolentArtState.Reset(base.Owner);
        return base.BeforeCombatStart();
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        ViolentArtState.Reset(base.Owner);
        return base.AfterCombatEnd(room);
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
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }
}

internal static class ViolentArtState
{
    private sealed class PlayerRecord
    {
        public object? CombatState;
        public int SpendCount;
    }

    private static readonly Dictionary<Player, PlayerRecord> Records = [];
    private static bool _subscribed;

    public static void EnsureSubscribed()
    {
        if (_subscribed)
        {
            return;
        }

        SunlightManager.SunlightSpent += OnSunlightSpent;
        _subscribed = true;
    }

    public static int GetSpendCount(Player? player)
    {
        return player == null ? 0 : GetRecord(player).SpendCount;
    }

    public static void Reset(Player? player)
    {
        if (player == null)
        {
            return;
        }

        GetRecord(player).SpendCount = 0;
    }

    private static PlayerRecord GetRecord(Player player)
    {
        EnsureSubscribed();
        object? combatState = player.Creature?.CombatState;
        if (!Records.TryGetValue(player, out PlayerRecord? record) ||
            !ReferenceEquals(record.CombatState, combatState))
        {
            record = new PlayerRecord
            {
                CombatState = combatState
            };
            Records[player] = record;
        }

        return record;
    }

    private static void OnSunlightSpent(Player player, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        GetRecord(player).SpendCount++;
    }
}
