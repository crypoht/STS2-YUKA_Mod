using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.Powers;

namespace KazamiYuka.Patches;

[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyHpLost))]
public class SunflowerGuardPatch
{
    public static void Postfix(
        IRunState runState,
        ICombatState combatState,
        Creature target, 
        decimal amount, 
        ValueProp props, 
        Creature dealer, 
        CardModel cardSource, 
        ref decimal __result) 
    {
        if (combatState == null) return;

        if (target.Monster is KazamiYukaSunflower && __result > 0)
        {
            _ = NotifyAfterDamageAsync(target, __result);
            return;
        }

        var player = target.Player;
        bool isMultiplayerCombat = combatState.Players.Count > 1;
        if (!target.IsPlayer ||
            player == null ||
            (isMultiplayerCombat && player.Character is not KazamiYukaCharacter) ||
            __result <= 0 ||
            props.HasFlag(ValueProp.Unblockable))
        {
            return;
        }

        var guardianSunflowers = SunflowerManager.GetSunflowers(player)
            .Where(c => c.IsAlive && c.Powers.Any(p => p is SunflowerGuardPower))
            .OrderBy(c => c.CurrentHp) 
            .ToList();
        bool hasFacingSun = player.Creature.HasPower<FacingSunPower>();

        GD.Print($"[向日葵挡刀测试] 玩家即将受到 {__result} 点伤害。场上带有守护状态的向日葵数量：{guardianSunflowers.Count}");

        foreach (var sunflower in guardianSunflowers)
        {
            if (__result <= 0) break;

            decimal absorbedDamage = Math.Min(
                __result,
                hasFacingSun ? sunflower.CurrentHp * 2m : sunflower.CurrentHp
            );
            decimal sunflowerDamage = hasFacingSun ? absorbedDamage * 0.5m : absorbedDamage;
            
            GD.Print($"[向日葵挡刀测试] 正在使用向日葵({sunflower.CurrentHp}血) 抵挡 {absorbedDamage} 点伤害，实际受到 {sunflowerDamage} 点伤害！");

            __result -= absorbedDamage;

            _ = ApplySunflowerDamageAsync(sunflower, sunflowerDamage);

            var damageVfx = NDamageNumVfx.Create(sunflower, (int)decimal.Truncate(sunflowerDamage), false);
            if (damageVfx != null)
            {
                var vfxContainer = sunflower.GetVfxContainer();
                if (vfxContainer != null)
                {
                    vfxContainer.AddChildSafely(damageVfx);
                }
                else if (NRun.Instance?.GlobalUi != null)
                {
                    NRun.Instance.GlobalUi.AddChildSafely(damageVfx);
                }
            }
        }
    }

    private static async Task ApplySunflowerDamageAsync(Creature sunflower, decimal damage)
    {
        await CreatureCmd.SetCurrentHp(sunflower, sunflower.CurrentHp - damage);
        await SunflowerManager.NotifyDamage(null!, sunflower, damage);
    }

    private static async Task NotifyAfterDamageAsync(Creature sunflower, decimal damage)
    {
        await Cmd.Wait(0.01f);
        await SunflowerManager.NotifyDamage(null!, sunflower, damage);
    }
}

[HarmonyPatch(typeof(NDamageNumVfx), nameof(NDamageNumVfx.Create), new[] { typeof(Creature), typeof(int), typeof(bool) })]
public static class SunflowerDamageNumberPatch
{
    public static void Postfix(Creature target, int damage, bool requireInteractable, ref NDamageNumVfx? __result)
    {
        if (__result == null && requireInteractable && damage > 0 && target.Monster is KazamiYukaSunflower)
        {
            __result = NDamageNumVfx.Create(target, damage, false);
        }
    }
}
