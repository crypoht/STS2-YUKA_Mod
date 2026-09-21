using System.Threading.Tasks;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class ClearSkyPower : ModPowerTemplate
{
    private bool _isTriggering;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/ClearSkyPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/ClearSkyPower.png"
    );

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
        if (_isTriggering || amount <= 0 || base.Amount <= 0 || myPlayer == null || targetPlayer != myPlayer) return;
        _ = TriggerExtraSunlight();
    }

    private async Task TriggerExtraSunlight()
    {
        if (_isTriggering || base.Amount <= 0) return;

        var player = base.Owner.Player;
        if (player == null) return;

        _isTriggering = true;
        try
        {
            base.Flash();
            SunlightManager.GainSunlight(player, 4);
            await PowerCmd.Apply<ClearSkyPower>(
                new ThrowingPlayerChoiceContext(),
                base.Owner,
                -1m,
                base.Owner,
                null,
                false
            );
        }
        finally
        {
            _isTriggering = false;
        }
    }
}