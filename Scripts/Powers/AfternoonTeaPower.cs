using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Models;

namespace KazamiYuka.Scripts.Powers;

// ================== 未升级版：随机一朵 ==================
[RegisterPower]
public sealed class AfternoonTeaRandomPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerAssetProfile AssetProfile => new("res://mods/KazamiYuka/images/powers/AfternoonTeaPower_small.png", "res://mods/KazamiYuka/images/powers/AfternoonTeaPower.png");

    // 👇 官方正确的挂载钩子
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SunlightManager.SunlightChanged += OnSunlightChanged;
        return base.AfterApplied(applier, cardSource);
    }

    // 👇 官方正确的移除钩子（极其重要：防止内存泄漏！）
    public override Task AfterRemoved(Creature oldOwner)
    {
        SunlightManager.SunlightChanged -= OnSunlightChanged;
        return base.AfterRemoved(oldOwner);
    }

    private void OnSunlightChanged(int oldSun, int newSun)
    {
        if (oldSun == newSun) return;
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

// ================== 升级版：所有向日葵 ==================
[RegisterPower]
public sealed class AfternoonTeaAllPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerAssetProfile AssetProfile => new("res://mods/KazamiYuka/images/powers/AfternoonTeaPower_small.png", "res://mods/KazamiYuka/images/powers/AfternoonTeaPower.png");

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SunlightManager.SunlightChanged += OnSunlightChanged;
        return base.AfterApplied(applier, cardSource);
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        SunlightManager.SunlightChanged -= OnSunlightChanged;
        return base.AfterRemoved(oldOwner);
    }

    private void OnSunlightChanged(int oldSun, int newSun)
    {
        if (oldSun == newSun) return;
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
