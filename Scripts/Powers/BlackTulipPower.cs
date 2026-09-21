using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class BlackTulipPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/BlackTulipPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/BlackTulipPower.png"
    );

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer?.Monster is not KazamiYukaSunflower)
        {
            return 1m;
        }

        return 1m + base.Amount * 0.25m;
    }

    public override async Task BeforeSideTurnEndEarly(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(base.Owner) || base.Owner.Player == null)
        {
            return;
        }

        foreach (Creature sunflower in SunflowerManager.GetSunflowers(base.Owner.Player).Where(flower => flower.IsAlive).ToList())
        {
            decimal maxHpLoss = Math.Min(base.Amount, Math.Max(0m, sunflower.MaxHp - 1m));
            if (maxHpLoss <= 0m)
            {
                continue;
            }

            await CreatureCmd.LoseMaxHp(choiceContext, sunflower, maxHpLoss, false);
            await SunflowerManager.NotifyMaxHpDecrease(choiceContext, sunflower, maxHpLoss);
        }
    }
}
