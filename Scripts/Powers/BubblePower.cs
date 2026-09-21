using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class BubblePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/BubblePower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/BubblePower.png"
    );

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != base.Owner.Side || !base.Owner.IsAlive)
        {
            return;
        }

        base.Flash();
        decimal maxHpLoss = Math.Min(base.Amount, Math.Max(0m, base.Owner.MaxHp - 1m));
        if (maxHpLoss > 0m)
        {
            await CreatureCmd.LoseMaxHp(choiceContext, base.Owner, maxHpLoss, false);
            await SunflowerManager.NotifyMaxHpDecrease(choiceContext, base.Owner, maxHpLoss);
        }

        await PowerCmd.Remove(this);
    }
}
