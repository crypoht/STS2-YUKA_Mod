using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class WarmthPower : ModPowerTemplate
{
    private int _spent;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => 8 - (_spent % 8);

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/WarmthPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/WarmthPower.png"
    );

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
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
        if (player != base.Owner.Player || amount <= 0)
        {
            return;
        }

        _spent += amount;
        int triggers = _spent / 8;
        _spent %= 8;
        if (triggers > 0)
        {
            _ = PlayerCmd.GainEnergy(triggers, player);
        }
        base.InvokeDisplayAmountChanged();
    }
}
