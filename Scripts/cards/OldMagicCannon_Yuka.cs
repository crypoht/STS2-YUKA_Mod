using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Cards;

[RegisterCard(typeof(KazamiYukaCardPool))]
public sealed class OldMagicCannon_Yuka : KazamiYukaCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(13m, ValueProp.Move),
        new DynamicVar("PreviousSunlight", 0m)
    ];

    public OldMagicCannon_Yuka() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
        OldMagicCannonState.EnsureSubscribed();
        OldMagicCannonState.Changed += OnStateChanged;
    }

    public override Task BeforeCombatStart()
    {
        OldMagicCannonState.Sync(base.Owner);
        return base.BeforeCombatStart();
    }

    private void OnStateChanged(Player player)
    {
        if (!IsCanonical && !HasBeenRemovedFromState && base.Owner == player)
        {
            DynamicVars["PreviousSunlight"].BaseValue = OldMagicCannonState.GetPrevious(player);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        Player player = base.Owner;
        OldMagicCannonState.Sync(player);

        if (base.CombatState == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(base.CombatState)
            .Execute(context);

        int sunlight = OldMagicCannonState.ConsumePrevious(player);
        if (sunlight > 0)
        {
            SunlightManager.GainSunlight(player, sunlight);
        }
    }

    protected override void OnUpgrade() => base.AddKeyword(CardKeyword.Retain);
}

internal static class OldMagicCannonState
{
    private sealed class PlayerRecord
    {
        public object? CombatState;
        public int LastTurn;
        public int CurrentSunlight;
        public int PreviousSunlight;
    }

    private static readonly Dictionary<Player, PlayerRecord> Records = [];
    private static bool _subscribed;
    public static event Action<Player>? Changed;

    public static void EnsureSubscribed()
    {
        if (_subscribed)
        {
            return;
        }

        SunlightManager.SunlightSpent += OnSunlightSpent;
        _subscribed = true;
    }

    public static void Sync(Player player)
    {
        EnsureSubscribed();
        PlayerRecord record = GetRecord(player);
        int turn = player.PlayerCombatState?.TurnNumber ?? record.LastTurn;
        if (turn <= record.LastTurn)
        {
            return;
        }

        record.PreviousSunlight = record.CurrentSunlight;
        record.CurrentSunlight = 0;
        record.LastTurn = turn;
        Changed?.Invoke(player);
    }

    public static int GetPrevious(Player player)
    {
        Sync(player);
        return GetRecord(player).PreviousSunlight;
    }

    public static int ConsumePrevious(Player player)
    {
        Sync(player);
        PlayerRecord record = GetRecord(player);
        int sunlight = record.PreviousSunlight;
        record.PreviousSunlight = 0;
        Changed?.Invoke(player);
        return sunlight;
    }

    private static PlayerRecord GetRecord(Player player)
    {
        EnsureSubscribed();
        object? combatState = player.PlayerCombatState;
        if (!Records.TryGetValue(player, out PlayerRecord? record) ||
            !ReferenceEquals(record.CombatState, combatState))
        {
            record = new PlayerRecord
            {
                CombatState = combatState,
                LastTurn = player.PlayerCombatState?.TurnNumber ?? 0
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

        PlayerRecord record = GetRecord(player);
        Sync(player);
        record.CurrentSunlight += amount;
        Changed?.Invoke(player);
    }
}
