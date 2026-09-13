using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace KazamiYuka.Scripts.Powers;

[RegisterPower]
public sealed class TyrantFormPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://mods/KazamiYuka/images/powers/TyrantFormPower.png",
        BigIconPath: "res://mods/KazamiYuka/images/powers/TyrantFormPower.png"
    );

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != base.Owner)
        {
            return;
        }

        if (cardPlay.Card is TyrantForm_Yuka)
        {
            return;
        }

        var player = base.Owner.Player;
        var combatState = base.Owner.CombatState;
        if (player == null || combatState == null)
        {
            return;
        }

        decimal totalMaxHpLoss = 0m;
        foreach (Creature sunflower in SunflowerManager.GetSunflowers(player))
        {
            if (!sunflower.IsAlive)
            {
                continue;
            }

            decimal maxHpLoss = Math.Min(4m, Math.Max(0m, sunflower.MaxHp - 1m));
            if (maxHpLoss <= 0m)
            {
                continue;
            }

            totalMaxHpLoss += maxHpLoss;
            await CreatureCmd.LoseMaxHp(choiceContext, sunflower, maxHpLoss, false);
        }

        if (totalMaxHpLoss <= 0m || base.Amount <= 0m)
        {
            return;
        }

        foreach (Creature enemy in combatState.Enemies)
        {
            if (!enemy.IsAlive)
            {
                continue;
            }

            await CreatureCmd.Damage(
                choiceContext,
                enemy,
                totalMaxHpLoss * base.Amount,
                ValueProp.Unpowered | ValueProp.Move,
                base.Owner
            );
        }
    }
}
