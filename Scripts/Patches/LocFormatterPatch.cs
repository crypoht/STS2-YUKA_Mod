using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using SmartFormat;
using KazamiYuka.Scripts.Formatters;

namespace KazamiYuka.Scripts.Patches
{
    // 拦截 LocManager 加载本地化解析器的方法
    [HarmonyPatch(typeof(LocManager), "LoadLocFormatters")]
    public static class LocFormatterPatch
    {
        public static void Postfix()
        {
            // 官方刚刚把它的 EnergyIcons 等解析器放进 Smart.Default，我们趁机追加一个！
            Smart.Default.AddExtensions(new SunlightIconsFormatter());
            
            Godot.GD.Print("[KazamiYuka] 成功向引擎底层注入 SunlightIconsFormatter 解析器！");
        }
    }
}