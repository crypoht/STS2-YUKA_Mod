using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using KazamiYuka.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class WarmthPower : ModPowerTemplate, IPowerExtraIconAmountLabelsProvider
{
    public const int SunlightThreshold = 8;

    private int _spent;
    private int _threshold = SunlightThreshold;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    public override int DisplayAmount => _threshold - _spent;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RemainingSunlight", SunlightThreshold),
        new EnergyVar(1)
    ];

    public IReadOnlyList<ExtraIconAmountLabelSlot> GetPowerExtraIconAmountLabelSlots()
    {
        return [
            ExtraIconAmountLabelSlot.At(
                ExtraIconAmountLabelCorner.BottomRight,
                DisplayAmount.ToString(),
                Colors.White,
                Colors.Black)
        ];
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/WarmthPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/WarmthPower.png"
    );

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _threshold = cardSource is Warmth_Yuka
            ? System.Math.Max(1, cardSource.DynamicVars["SunlightThreshold"].IntValue)
            : SunlightThreshold;
        _spent = 0;
        DynamicVars["RemainingSunlight"].BaseValue = DisplayAmount;
        SunlightManager.SunlightSpent -= OnSunlightSpent;
        SunlightManager.SunlightSpent += OnSunlightSpent;
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        SunlightManager.SunlightSpent -= OnSunlightSpent;
        return Task.CompletedTask;
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        SunlightManager.SunlightSpent -= OnSunlightSpent;
        _spent = 0;
        await PowerCmd.Remove(this);
    }

    private void OnSunlightSpent(MegaCrit.Sts2.Core.Entities.Players.Player player, int amount)
    {
        if (player != base.Owner.Player || !base.Owner.IsAlive || amount <= 0)
        {
            return;
        }

        _spent += amount;
        int triggers = _spent / _threshold;
        _spent %= _threshold;
        DynamicVars["RemainingSunlight"].BaseValue = DisplayAmount;
        if (triggers > 0)
        {
            _ = PlayerCmd.GainEnergy(triggers * DynamicVars["Energy"].BaseValue, player);
        }
        base.InvokeDisplayAmountChanged();
    }
}
