using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using KazamiYuka.Scripts.Cards;

namespace KazamiYuka.Scripts.Patches
{
    [HarmonyPatch(typeof(CardModel))]
    public static class CardCostFilterPatch
    {
        // 拦截官方的 CostsEnergyOrStars 检测
        [HarmonyPatch(nameof(CardModel.CostsEnergyOrStars))]
        [HarmonyPostfix]
        public static void CostsEnergyOrStarsPostfix(CardModel __instance, ref bool __result)
        {
            // 如果官方判定这张卡“不耗费能量”，准备把它扔出药水选择界面时 (__result == false)
            if (!__result && __instance is KazamiYukaCard yukaCard)
            {
                // 我们去摸一下这张卡的口袋，看它需不需要消耗阳光
                if (yukaCard.DynamicVars.TryGetValue("SunlightCost", out var sunCostVar) && sunCostVar.BaseValue > 0m)
                {
                    // 如果消耗阳光，强行告诉药水引擎：这卡要花钱！给我显示出来！
                    __result = true; 
                }
            }
        }
    }
}