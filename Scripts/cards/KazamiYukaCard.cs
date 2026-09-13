using System;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Entities.Players;
using KazamiYuka.Scripts;
using Godot;

namespace KazamiYuka.Scripts.Cards;

public abstract class KazamiYukaCard : ModCardTemplate
{
    private const string CardPortraitDirectory = "res://mods/KazamiYuka/images/cards";
    private const string TemporaryCardPortraitPath = $"{CardPortraitDirectory}/Temp_Yuka.png";

    public override CardAssetProfile AssetProfile => DefaultCardAssetProfile;

    protected CardAssetProfile DefaultCardAssetProfile => new(
        PortraitPath: ResolveCardPortraitPath()
    );

    private string ResolveCardPortraitPath()
    {
        string portraitPath = $"{CardPortraitDirectory}/{GetType().Name}.png";
        return ResourceLoader.Exists(portraitPath)
            ? portraitPath
            : TemporaryCardPortraitPath;
    }

    // =======================================================
    // 👇 恢复这个公开属性！让你的 UI 图标也能直接读取它！
    // =======================================================
    public bool IsSunlightFree 
{
    get 
    {
        // 1. 读取官方能量成本的当前状态
        var energy = base.EnergyCost;
        if (energy == null) return false;

        // 2. 官方判定：卡牌现在被强行设为了 0 费 (不论是药水、遗物还是 Buff 导致的)
        // GetResolved() 会返回最终生效的费用（扣除所有 modifier 后的值）
        if (energy.GetResolved() == 0)
        {
            // 如果它本来是有费用的 (Canonical > 0)，现在变成 0，说明它被免单了
            // 或者如果它本来就是 0 费，但被某种特效修改过 (HasLocalModifiers)
            if (energy.Canonical > 0 || energy.HasLocalModifiers) return true;
        }
        
        return false;
    }
}

    protected KazamiYukaCard(int cost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary) 
        : base(cost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override bool IsPlayable
    {
        get
        {
           if (!base.IsPlayable) return false;

            // 拦截：手牌里如果已经免费，直接防卡手放行！
            if (this.IsSunlightFree) return true;

            if (DynamicVars.TryGetValue("SunlightCost", out var sunCostVar))
            {
                var player = base.Owner as Player;
                if (player != null)
                {
                    int requiredSunlight = (int)sunCostVar.BaseValue;
                    if (!SunlightManager.HasEnoughSunlight(player, requiredSunlight))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    // ==========================================================
    // 👇 核心修复：完善 0 费卡牌的收据判定！
    // ==========================================================
    protected bool IsFreeFromReceipt(CardPlay cardPlay)
    {
        if (cardPlay == null || cardPlay.Card?.EnergyCost == null) return false;

        // 如果收据显示实际消耗了 0 能量
        if (cardPlay.Resources.EnergySpent == 0)
        {
            // 条件1：卡牌本来是要花能量的 (如 SunlightStrike_Yuka, Canonical=1)
            if (cardPlay.Card.EnergyCost.Canonical > 0) return true;

            // 条件2：卡牌本来就是 0 费 (如 FlowerBedProtection_Yuka, Canonical=0)
            // 但是它身上被药水/水花贴了“本回合免费”的临时标签！
            if (cardPlay.Card.EnergyCost.HasLocalModifiers) return true;
        }
        return false;
    }

    // 接口 A
    protected bool TryPayFixedSunlight(Player player, int requiredAmount, CardPlay cardPlay)
    {
        if (IsFreeFromReceipt(cardPlay)) return true; // 查收据，免单放行！
        return SunlightManager.TrySpendSunlight(player, requiredAmount);
    }

    // 接口 B
    protected int ConsumeUpToMaxSunlight(Player player, int maxLimit, CardPlay cardPlay)
    {
        if (IsFreeFromReceipt(cardPlay)) return maxLimit; // 查收据，免单当最大值！

        int sunAvailable = SunlightManager.CurrentSunlight;
        int sunToSpend = Math.Min(sunAvailable, maxLimit);
        
        if (sunToSpend > 0) SunlightManager.TrySpendSunlight(player, sunToSpend);
        
        return sunToSpend;
    }
}
