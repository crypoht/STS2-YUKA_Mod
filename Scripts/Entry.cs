using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib; 
using STS2RitsuLib.Interop; 
using STS2RitsuLib.Localization.SmartFormat;
using System.Reflection; 
using Godot;
using KazamiYuka.Scripts.Formatters;

namespace KazamiYuka.Scripts;

[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "KazamiYuka";
    
    public static readonly MegaCrit.Sts2.Core.Logging.Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    

    public static void Init()
    {
        _ = KazamiYukaTargetTypes.Sunflower;

        KazamiYuka.Scripts.SunflowerManager.RegisterHealthListener(KazamiYuka.Scripts.Powers.FlowerGratitudePower.OnAnySunflowerHealthChanged);
        KazamiYuka.Scripts.SunflowerManager.RegisterHealthListener(KazamiYuka.Scripts.Powers.FlowerMoundPower.OnAnySunflowerHealthChanged);
        KazamiYuka.Scripts.SunflowerManager.RegisterHealthListener(KazamiYuka.Scripts.Powers.SunflowerFieldPower.OnSunflowerHealthChanged);
        KazamiYuka.Scripts.SunflowerManager.RegisterHealthListener(KazamiYuka.Scripts.Cards.FlowingWind_Yuka.OnAnySunflowerHealthChanged);

        try
    {
        MegaCrit.Sts2.Core.Assets.PreloadManager.Cache.GetTexture2D("res://mods/KazamiYuka/images/sunlight_icon.png");
        GD.Print("[KazamiYuka] 阳光图标已成功预先加载到缓存中！");
    }
    catch (System.Exception e)
    {
        GD.PrintErr($"[KazamiYuka] 阳光图标加载失败！请检查图片和 import 文件！错误: {e.Message}");
    }

        var harmony = new Harmony("sts2.mod.kazamiyuka");
        harmony.PatchAll();
        
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);

        var assembly = Assembly.GetExecutingAssembly();
        
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        ModSmartFormatExtensionRegistry.For(ModId).Register<SunlightIconsFormatter>();
        
        Log.Info("KazamiYuka Mod (RitsuLib Mode) initialized successfully!");
    }
}
