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

            if (cardPlay.IsAutoPlay || KazamiYukaCard.IsEnergyCostTemporarilyFree(cardModel)) return true;

            return SunlightManager.TrySpendSunlight(player, cost);
        }

        public static int ConsumeVariableSunlight(this IVariableSunlightCard card, Player player, CardPlay cardPlay)
        {
            int max = card.GetMaxSunlightConsume();
            var cardModel = card as CardModel;
            if (cardModel == null) return 0;

            if (cardPlay.IsAutoPlay || KazamiYukaCard.IsEnergyCostTemporarilyFree(cardModel))
            {
                return Math.Min(SunlightManager.GetSunlight(player), max);
            }

            int actualSpent = Math.Min(SunlightManager.GetSunlight(player), max);
            
            if (actualSpent > 0)
            {
                SunlightManager.TrySpendSunlight(player, actualSpent);
            }
            
            return actualSpent;
        }
    }
}
