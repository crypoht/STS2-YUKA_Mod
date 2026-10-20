using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class ComposurePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/ComposurePower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/ComposurePower.png"
    );

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != base.Owner.Player || base.Amount <= 0m)
        {
            return;
        }

        base.Flash();
        await PlayerCmd.GainEnergy(1m, player);
        await PowerCmd.Apply<ComposurePower>(
            new ThrowingPlayerChoiceContext(),
            base.Owner,
            -1m,
            base.Owner,
            null,
            false
        );
    }
}
