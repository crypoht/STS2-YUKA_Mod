using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Models;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class AfternoonTeaRandomPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerAssetProfile AssetProfile => new("res://mods/KazamiYuka/images/powers/AfternoonTeaPower_small.png", "res://mods/KazamiYuka/images/powers/AfternoonTeaPower.png");

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SunlightManager.SunlightGained += OnSunlightGained;
        return base.AfterApplied(applier, cardSource);
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        SunlightManager.SunlightGained -= OnSunlightGained;
        return base.AfterRemoved(oldOwner);
    }

    private void OnSunlightGained(Player targetPlayer, int amount)
    {
        var myPlayer = base.Owner.Player;
        if (myPlayer == null || targetPlayer != myPlayer) return;
        _ = TriggerMaxHpGain();
    }

    private async Task TriggerMaxHpGain()
    {
        var player = base.Owner.Player;
        if (player == null) return;
        var flowers = SunflowerManager.GetSunflowers(player);
        if (flowers.Count == 0) return;

        int hpToGain = (int)base.Amount;
        var target = flowers[Random.Shared.Next(flowers.Count)];
        
        await SunflowerManager.GainMaxHpSingle(null!, target, hpToGain);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == base.Owner.Side) await PowerCmd.Remove(this);
    }
}

[RegisterPower]
public sealed class AfternoonTeaAllPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerAssetProfile AssetProfile => new("res://mods/KazamiYuka/images/powers/AfternoonTeaPower_small.png", "res://mods/KazamiYuka/images/powers/AfternoonTeaPower.png");

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SunlightManager.SunlightGained += OnSunlightGained;
        return base.AfterApplied(applier, cardSource);
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        SunlightManager.SunlightGained -= OnSunlightGained;
        return base.AfterRemoved(oldOwner);
    }

    private void OnSunlightGained(Player targetPlayer, int amount)
    {
        var myPlayer = base.Owner.Player;
        if (myPlayer == null || targetPlayer != myPlayer) return;
        _ = TriggerMaxHpGain();
    }

    private async Task TriggerMaxHpGain()
    {
        var player = base.Owner.Player;
        if (player == null) return;
        var flowers = SunflowerManager.GetSunflowers(player);
        if (flowers.Count == 0) return;

        int hpToGain = (int)base.Amount;
        foreach (var target in flowers)
        {
            await SunflowerManager.GainMaxHpSingle(null!, target, hpToGain);
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == base.Owner.Side) await PowerCmd.Remove(this);
    }
}