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
using KazamiYuka.Scripts.Patches;

namespace KazamiYuka.Scripts;

[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "KazamiYuka";
    
    public static readonly MegaCrit.Sts2.Core.Logging.Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    

    public static void Init()
    {
        GD.Print("[KazamiYuka][CombatTrace] Entry.Init begin.");
        _ = KazamiYukaTargetTypes.Sunflower;

        KazamiYuka.Scripts.SunflowerManager.RegisterHealthListener(KazamiYuka.Scripts.Powers.FlowerGratitudePower.OnAnySunflowerHealthChanged);
        KazamiYuka.Scripts.SunflowerManager.RegisterHealthListener(KazamiYuka.Scripts.Powers.FlowerMoundPower.OnAnySunflowerHealthChanged);
        KazamiYuka.Scripts.SunflowerManager.RegisterHealthListener(KazamiYuka.Scripts.Powers.SunflowerFieldPower.OnSunflowerHealthChanged);
        KazamiYuka.Scripts.SunflowerManager.RegisterHealthListener(KazamiYuka.Scripts.Cards.FlowingWind_Yuka.OnAnySunflowerHealthChanged);
        RitsuLibFramework.RegisterCardOnPlayHookListener(new YukaPowerCastAnimationHook());

        try
    {
        MegaCrit.Sts2.Core.Assets.PreloadManager.Cache.GetTexture2D("res://mods/KazamiYuka/images/sunlight_icon.png");
        GD.Print("[KazamiYuka] 阳光图标已成功预先加载到缓存中！");
    }
    catch (System.Exception e)
    {
        GD.PrintErr($"[KazamiYuka] 阳光图标加载失败！请检查图片和 import 文件！错误: {e.Message}");
    }

        ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);

        var assembly = Assembly.GetExecutingAssembly();
        
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

        var harmony = new Harmony("sts2.mod.kazamiyuka");
        harmony.PatchAll();
        
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        ModSmartFormatExtensionRegistry.For(ModId).Register<SunlightIconsFormatter>();
        
        GD.Print("[KazamiYuka][CombatTrace] Entry.Init complete.");
        Log.Info("KazamiYuka Mod (RitsuLib Mode) initialized successfully!");
    }
}
