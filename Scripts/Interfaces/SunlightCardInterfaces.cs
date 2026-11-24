using System;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KazamiYuka.Scripts;
using KazamiYuka.Scripts.Cards;
using MegaCrit.Sts2.Core.Models;

namespace KazamiYuka.Scripts.Interfaces
{
    public interface IFixedSunlightCard
    {
        int GetSunlightCost();
    }

    public interface IVariableSunlightCard
    {
        int GetMaxSunlightConsume();
    }

    public static class SunlightCardExtensions
    {
        public static bool TryPayFixedSunlight(this IFixedSunlightCard card, Player player, CardPlay cardPlay)
        {
            int cost = card.GetSunlightCost();
            var cardModel = card as CardModel; 
            if (cardModel == null) return false;

            if (cardPlay.IsAutoPlay
                || !cardPlay.IsFirstInSeries
                || KazamiYukaCard.IsEnergyCostTemporarilyFree(cardModel)) return true;

            return SunlightManager.TrySpendSunlight(player, cost);
        }

        public static int ConsumeVariableSunlight(this IVariableSunlightCard card, Player player, CardPlay cardPlay)
        {
            int max = card.GetMaxSunlightConsume();
            var cardModel = card as CardModel;
            if (cardModel == null) return 0;

            if (cardPlay.IsAutoPlay
                || !cardPlay.IsFirstInSeries
                || KazamiYukaCard.IsEnergyCostTemporarilyFree(cardModel))
            {
                return max;
            }

            int actualSpent = Math.Min(SunlightManager.GetFierceSunlight(player), max);
            
            if (actualSpent > 0 && SunlightManager.GetSunlight(player) > 0)
            {
                SunlightManager.TrySpendSunlight(player, actualSpent);
            }
            
            return actualSpent;
        }
    }
}
