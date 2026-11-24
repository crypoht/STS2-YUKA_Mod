using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class FlowerMoundPower : ModPowerTemplate
{
    public const int SunlightPerStack = 4;
    public const int EnergyPerStack = 1;

    private int _pendingStacks;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("SunlightGain", SunlightPerStack),
        new EnergyVar(EnergyPerStack)
    ];

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/FlowerMoundPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/FlowerMoundPower.png"
    );

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        UpdateRewardDescription();
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power == this)
        {
            UpdateRewardDescription();
        }

        return Task.CompletedTask;
    }

    private void UpdateRewardDescription()
    {
        DynamicVars["SunlightGain"].BaseValue = Amount * SunlightPerStack;
        DynamicVars["Energy"].BaseValue = Amount * EnergyPerStack;
    }

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player || !Owner.IsAlive || _pendingStacks <= 0)
        {
            return;
        }

        int pendingStacks = _pendingStacks;
        _pendingStacks = 0;
        Flash();
        SunlightManager.GainSunlight(player, pendingStacks * SunlightPerStack);
        await PlayerCmd.GainEnergy(pendingStacks * EnergyPerStack, player);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _pendingStacks = 0;
        return Task.CompletedTask;
    }

    public static Task OnAnySunflowerHealthChanged(
        MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext ctx,
        Creature sunflower,
        FlowerHealthEvent eventType,
        decimal amount)
    {
        if (eventType != FlowerHealthEvent.Damage || amount <= 0m)
        {
            return Task.CompletedTask;
        }

        foreach (var player in SunflowerManager.GetCombatPlayers(sunflower))
        {
            FlowerMoundPower? power = player.Creature.GetPower<FlowerMoundPower>();
            if (power != null && player.Creature.IsAlive && power.Amount > 0)
            {
                power._pendingStacks += power.Amount;
            }
        }

        return Task.CompletedTask;
    }
}
