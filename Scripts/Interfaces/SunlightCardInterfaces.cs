using System;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KazamiYuka.Scripts;
using MegaCrit.Sts2.Core.Models; // 确保引入 SunlightManager

namespace KazamiYuka.Scripts.Interfaces
{
    // ==========================================
    // 1. 定义接口：强迫卡牌声明自己的阳光参数
    // ==========================================

    /// <summary>
    /// 固定消耗阳光的卡牌接口
    /// </summary>
    public interface IFixedSunlightCard
    {
        int GetSunlightCost();
    }

    /// <summary>
    /// 最多消耗(X)阳光的卡牌接口
    /// </summary>
    public interface IVariableSunlightCard
    {
        int GetMaxSunlightConsume();
    }

    public static class SunlightCardExtensions
    {
        // 给所有实现了 IFixedSunlightCard 的卡牌添加 TryPayFixedSunlight 方法
        public static bool TryPayFixedSunlight(this IFixedSunlightCard card, Player player, CardPlay cardPlay)
        {
            int cost = card.GetSunlightCost();
            var cardModel = card as CardModel; 
            if (cardModel == null) return false;

            // 查验是否免费打出
            bool isFree = false;
            if (cardModel.EnergyCost != null && cardModel.EnergyCost.GetResolved() == 0)
            {
                if (cardModel.EnergyCost.Canonical > 0 || cardModel.EnergyCost.HasLocalModifiers) isFree = true;
            }

            if (isFree) return true;

            // 👇 完美调用你写好的 TrySpendSunlight 方法！
            return SunlightManager.TrySpendSunlight(player, cost);
        }

        // 给所有实现了 IVariableSunlightCard 的卡牌添加 ConsumeVariableSunlight 方法
        public static int ConsumeVariableSunlight(this IVariableSunlightCard card, Player player, CardPlay cardPlay)
        {
            int max = card.GetMaxSunlightConsume();
            var cardModel = card as CardModel;
            if (cardModel == null) return 0;

            bool isFree = false;
            if (cardModel.EnergyCost != null && cardModel.EnergyCost.GetResolved() == 0)
            {
                if (cardModel.EnergyCost.Canonical > 0 || cardModel.EnergyCost.HasLocalModifiers) isFree = true;
            }

            if (isFree) return max;

            // 👇 算出实际能消耗多少阳光 (取现有阳光和最大上限的最小值)
            int actualSpent = Math.Min(SunlightManager.CurrentSunlight, max);
            
            // 👇 调用你的方法真实扣除阳光
            if (actualSpent > 0)
            {
                SunlightManager.TrySpendSunlight(player, actualSpent);
            }
            
            return actualSpent;
        }
    }
}