using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Combat;
using KazamiYuka.Scripts.Nodes; 
// 👇 确保引用了 SunlightManager 的命名空间
using KazamiYuka.Scripts; 

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi.Activate))]
public static class InjectSunlightCounterPatch
{
    public static void Postfix(NCombatUi __instance, CombatState state)
    {
        // ==========================================
        // 👇 核心修复：借鸡生蛋！在这里顺手把阳光清空！
        // ==========================================
        SunlightManager.ResetForCombat();
        GD.Print("[KazamiYuka] 战斗开始！阳光已重置为 0。");

        // 1. 动态加载阳光场景（注意必须是绝对正确的 res://mods/ 路径）
        PackedScene scene = GD.Load<PackedScene>("res://mods/KazamiYuka/scenes/SunlightCounter.tscn");
        
        if (scene != null)
        {
            // 2. 实例化节点
            NSunlightCounter sunlightUi = scene.Instantiate<NSunlightCounter>();
            
            // 3. 安全注入：挂载到官方大管家的 EnergyCounterContainer 下面
            __instance.EnergyCounterContainer.CallDeferred(Node.MethodName.AddChild, sunlightUi);

            // 4. 调整位置！
            sunlightUi.Position = new Vector2(-48f, -47f); 
            
            GD.Print("[KazamiYuka] 阳光计数器已全局注入到当前角色的战斗 UI 中！");
        }
        else
        {
            GD.PrintErr("[KazamiYuka] 严重错误：找不到阳光计数器的场景文件！");
        }
    }
}