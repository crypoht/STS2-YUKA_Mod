using System;
using System.Linq;
using System.Collections.Generic;
using Godot; // 引入 Godot 用于打日志
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Runs;
using KazamiYuka.Scripts; // 引入你的 SunflowerManager
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
            _ = SunflowerManager.NotifyDamage(null!, target, __result);
            return;
        }

        // 【修复点】：显式提取 target.Player，并检查它是否为 null，让编译器闭嘴
        var player = target.Player;
        if (!target.IsPlayer || player == null || __result <= 0 || props.HasFlag(ValueProp.Unblockable)) return;

        // 既然上面确保了 player 绝对不为 null，这里传进去就没有任何警告了！
        var guardianSunflowers = SunflowerManager.GetSunflowers(player)
            .Where(c => c.IsAlive && c.Powers.Any(p => p is SunflowerGuardPower))
            .OrderBy(c => c.CurrentHp) 
            .ToList();

        GD.Print($"[向日葵挡刀测试] 玩家即将受到 {__result} 点伤害。场上带有守护状态的向日葵数量：{guardianSunflowers.Count}");

        foreach (var sunflower in guardianSunflowers)
        {
            if (__result <= 0) break;

            decimal absorbAmount = Math.Min(__result, sunflower.CurrentHp);
            
            GD.Print($"[向日葵挡刀测试] 正在使用向日葵({sunflower.CurrentHp}血) 抵挡 {absorbAmount} 点伤害！");

            __result -= absorbAmount;

            var newHp = sunflower.CurrentHp - absorbAmount;
            _ = CreatureCmd.SetCurrentHp(sunflower, newHp);
            _ = SunflowerManager.NotifyDamage(null!, sunflower, absorbAmount);
        }
    }
}
