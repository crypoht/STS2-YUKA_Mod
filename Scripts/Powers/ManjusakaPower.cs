using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class ManjusakaPower : ModPowerTemplate
{
    private Creature? _pendingSunflower;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/ManjusakaPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/ManjusakaPower.png"
    );

    public override decimal ModifyHpLostAfterOstyLate(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (!target.IsPlayer ||
            !target.IsAlive ||
            amount <= 0m ||
            amount < target.CurrentHp ||
            _pendingSunflower != null)
        {
            return amount;
        }

        _pendingSunflower = SunflowerManager.GetAllCombatSunflowers(target)
            .Select((sunflower, index) => new { sunflower, index })
            .OrderBy(entry => entry.sunflower.CurrentHp)
            .ThenByDescending(entry => entry.index)
            .Select(entry => entry.sunflower)
            .FirstOrDefault();

        return _pendingSunflower == null ? amount : 0m;
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        Creature? sunflower = _pendingSunflower;
        _pendingSunflower = null;
        if (sunflower == null || sunflower.IsDead)
        {
            return;
        }

        base.Flash();
        await CreatureCmd.Kill(sunflower);
    }
}
