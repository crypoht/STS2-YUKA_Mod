using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class WinterBloomPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/WinterBloomPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/WinterBloomPower.png"
    );

    public override async Task BeforeSideTurnEndEarly(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != base.Owner.Side || !base.Owner.IsAlive)
        {
            return;
        }

        foreach (Player player in base.Owner.CombatState?.Players
                     .Where(player => player.Creature.IsAlive &&
                                      player.Creature.GetPower<FourSeasonsBloomPower>() != null)
                     .ToList() ?? [])
        {
            await CreatureCmd.GainBlock(
                player.Creature,
                base.Amount,
                ValueProp.Unpowered,
                null,
                false
            );
        }
    }
}
