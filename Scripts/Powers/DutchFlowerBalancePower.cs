using System.Collections.Generic;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Combat;
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
public sealed class DutchFlowerBalancePower : ModPowerTemplate
{
    private bool _isTriggering;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/DutchFlowerBalancePower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/DutchFlowerBalancePower.png"
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

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(base.Owner))
        {
            return;
        }

        await PowerCmd.Apply<DutchFlowerBalancePower>(
            choiceContext,
            base.Owner,
            -1m,
            base.Owner,
            null,
            false
        );
    }

    private void OnSunlightSpent(Player player, int amount)
    {
        if (_isTriggering || amount <= 0 || player != base.Owner.Player || base.Amount <= 0)
        {
            return;
        }

        _isTriggering = true;
        try
        {
            SunlightManager.GainSunlight(player, 2);
        }
        finally
        {
            _isTriggering = false;
        }
    }
}
