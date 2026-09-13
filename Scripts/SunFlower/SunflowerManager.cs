using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace KazamiYuka.Scripts;

public enum FlowerHealthEvent
    {
        Heal,           // 生命回复
        Damage,         // 生命减少 (受伤)
        MaxHpIncrease,  // 上限增加
        MaxHpDecrease   // 上限减少
    }

    public delegate Task FlowerHealthChangedHandler(PlayerChoiceContext ctx, Creature sunflower, FlowerHealthEvent eventType, decimal amount);

public static class SunflowerManager
{
    
    public static List<Creature> GetSunflowers(Player player)
    {
        return player?.Creature?.CombatState?.Allies?
            .Where(c => c.Monster is KazamiYukaSunflower && c.PetOwner == player)
            .ToList() ?? new List<Creature>();
    }

    private static readonly Vector2[] SunflowerSlotOffsets =
    [
        new Vector2(0f, -110f),
        new Vector2(220f, -120f),
        new Vector2(440f, -105f),
        new Vector2(110f, 18f),
        new Vector2(330f, 4f)
    ];

    public static async Task Summon(Player summoner, decimal initialHp, int maxSunflowers = 5)
    {
        if (summoner == null) return;

        var existingSunflowers = GetSunflowers(summoner);

        // ========== 溢出处理：场上已满时，给所有向日葵加上一半数值的最大生命 ==========
        if (existingSunflowers.Count >= maxSunflowers)
        {
            decimal overflowMaxHp = initialHp / 2m;
            if (overflowMaxHp > 0)
            {
                await GainMaxHpAll(null!, summoner, overflowMaxHp);
            }
            return;
        }

        decimal surroundBonusMaxHp = SurroundPower.GetBonusMaxHp(summoner);
        Creature newFlower = await PlayerCmd.AddPet<KazamiYukaSunflower>(summoner);
        
        await CreatureCmd.SetMaxHp(newFlower, initialHp + surroundBonusMaxHp);
        await CreatureCmd.Heal(newFlower, initialHp + surroundBonusMaxHp, true);

        ArrangeSunflowers(summoner);

        KazamiYukaStarterRelic? starterRelic = null;
        if (summoner.Relics != null)
        {
            foreach (var relic in summoner.Relics)
            {
                if (relic is KazamiYukaStarterRelic yukaRelic)
                {
                    starterRelic = yukaRelic;
                    break;
                }
            }
        }

        

        if (CombatManager.Instance != null && CombatManager.Instance.IsInProgress)
        {
            SfxCmd.Play("event:/sfx/player/defect/orb_dark_evoke", 1f);
        }

        if (newFlower != null)
        {
            await PowerCmd.Apply<XiangYangPower>(
                new ThrowingPlayerChoiceContext(),
                newFlower,          
                1,                  
                summoner.Creature,  
                null                
            );

            if (surroundBonusMaxHp > 0)
            {
                await PowerCmd.Apply<SunflowerGuardPower>(
                    new ThrowingPlayerChoiceContext(),
                    newFlower,
                    1m,
                    summoner.Creature,
                    null,
                    false
                );
            }
        }
    }
    public static void ArrangeSunflowers(Player summoner)
    {
        ArrangeSunflowers(summoner, GetSunflowers(summoner));
    }

    private static void ArrangeSunflowers(Player summoner, IReadOnlyList<Creature> sunflowers)
    {
        NCombatRoom? room = NCombatRoom.Instance;
        NCreature? playerNode = room?.GetCreatureNode(summoner.Creature);
        if (room == null || playerNode == null)
        {
            return;
        }

        Vector2 playerRelativeOrigin = playerNode.Position + new Vector2(
            playerNode.Hitbox.Size.X * 0.5f + 90f,
            20f
        );

        for (int index = 0; index < sunflowers.Count; index++)
        {
            Creature flower = sunflowers[index];
            NCreature? flowerNode = room.GetCreatureNode(flower);
            if (flowerNode == null)
            {
                continue;
            }

            Vector2 offset = SunflowerSlotOffsets[index % SunflowerSlotOffsets.Length];
            flowerNode.Position = playerRelativeOrigin + offset;
            SetSunflowerLayer(flowerNode, playerNode);
            room.SetCreatureIsInteractable(flower, true);

            if (flowerNode.Visuals?.Bounds != null)
            {
                flowerNode.Visuals.Bounds.Visible = true;
            }
        }
    }

    private static void SetSunflowerLayer(NCreature flowerNode, NCreature playerNode)
    {
        flowerNode.ZAsRelative = true;
        flowerNode.ZIndex = 1;

        if (flowerNode.Visuals != null)
        {
            flowerNode.Visuals.ZAsRelative = true;
            flowerNode.Visuals.ZIndex = 0;
        }
    }

    private static readonly List<FlowerHealthChangedHandler> _healthListeners = new();

    // 任何想监听向日葵血量变化的类，调用这个方法注册自己
    public static void RegisterHealthListener(FlowerHealthChangedHandler handler)
    {
        if (!_healthListeners.Contains(handler))
        {
            _healthListeners.Add(handler);
        }
    }

    // 内部广播方法：依次通知所有订阅了的监听器
    private static async Task NotifyHealthChanged(PlayerChoiceContext ctx, Creature sunflower, FlowerHealthEvent eventType, decimal amount)
    {
        foreach (var listener in _healthListeners)
        {
            await listener(ctx, sunflower, eventType, amount);
        }
    }

    public static Task NotifyDamage(PlayerChoiceContext ctx, Creature sunflower, decimal amount)
    {
        return NotifyHealthChanged(ctx, sunflower, FlowerHealthEvent.Damage, amount);
    }

    // =========================================================
    // 👇 4. 修改你原来的方法，让它们在执行后自动广播！
    // =========================================================
    public static async Task HealSingle(PlayerChoiceContext choiceContext, Creature sunflower, decimal amount)
    {
        if (sunflower != null && sunflower.IsAlive)
        {
            await CreatureCmd.Heal(sunflower, amount, true);
            await NotifyHealthChanged(choiceContext, sunflower, FlowerHealthEvent.Heal, amount);
        }
    }

    // 修复：让群体回血也能顺利触发单体广播
    public static async Task HealAll(PlayerChoiceContext choiceContext, Player summoner, decimal amount)
    {
        foreach (var flower in GetSunflowers(summoner))
        {
            await HealSingle(choiceContext, flower, amount);
        }
    }

    public static async Task GainMaxHpSingle(PlayerChoiceContext choiceContext, Creature sunflower, decimal amount)
    {
        if (sunflower != null && sunflower.IsAlive)
        {
            await CreatureCmd.GainMaxHp(sunflower, amount);
            await NotifyHealthChanged(choiceContext, sunflower, FlowerHealthEvent.MaxHpIncrease, amount);
        }
    }

    // 修复：让群体加生命上限也能顺利触发单体广播
    public static async Task GainMaxHpAll(PlayerChoiceContext choiceContext, Player summoner, decimal amount)
    {
        foreach (var flower in GetSunflowers(summoner))
        {
            await GainMaxHpSingle(choiceContext, flower, amount);
        }
    }
}
