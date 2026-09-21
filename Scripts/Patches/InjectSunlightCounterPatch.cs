using HarmonyLib;
using Godot;
using System.Collections.Generic;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using KazamiYuka.Scripts.Nodes;
using KazamiYuka.Scripts;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi.Activate))]
public static class InjectSunlightCounterPatch
{
    private static NSunlightCounter? _energyCounter;
    private static readonly List<NMultiplayerSunlightCounter> _multiplayerPlayerCounters = new();

    public static void Postfix(NCombatUi __instance, CombatState state)
    {
        GD.Print("[InjectSunlightCounterPatch] Postfix called");

        SunlightManager.ResetForCombat();
        SunflowerManager.ResetCombatState();
        GD.Print("[KazamiYuka] 战斗开始！阳光已重置为 0。");

        var localPlayer = MegaCrit.Sts2.Core.Context.LocalContext.GetMe(state);
        if (localPlayer == null)
        {
            return;
        }

        if (!SunlightManager.IsYukaPlayer(localPlayer) && state.Players.Count <= 1)
        {
            return;
        }

        if ((_energyCounter != null && GodotObject.IsInstanceValid(_energyCounter)) ||
            _multiplayerPlayerCounters.Exists(counter => GodotObject.IsInstanceValid(counter)))
        {
            GD.Print("[InjectSunlightCounterPatch] Counter already exists, skipping injection");
            return;
        }

        PackedScene? scene = GD.Load<PackedScene>("res://mods/KazamiYuka/scenes/SunlightCounter.tscn");

        if (scene == null)
        {
            GD.PrintErr("[KazamiYuka] 严重错误：找不到阳光计数器的场景文件！");
            return;
        }

        GD.Print("[InjectSunlightCounterPatch] Scene loaded successfully");

        NSunlightCounter? sunlightUi = scene.Instantiate<NSunlightCounter>();

        if (sunlightUi == null)
        {
            GD.PrintErr("[InjectSunlightCounterPatch] Failed to instantiate NSunlightCounter");
            return;
        }

        GD.Print("[InjectSunlightCounterPatch] NSunlightCounter instantiated");

        sunlightUi.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
        sunlightUi.Position = new Vector2(-35f, -40f);
        __instance.EnergyCounterContainer.AddChild(sunlightUi);
        _energyCounter = sunlightUi;

        if (state.Players.Count > 1)
        {
            NMultiplayerPlayerStateContainer? playerStates =
                NRun.Instance?.GlobalUi?.MultiplayerPlayerContainer;

            if (playerStates != null)
            {
                foreach (Node child in playerStates.GetChildren())
                {
                    if (child is not NMultiplayerPlayerState playerState)
                    {
                        continue;
                    }

                    HBoxContainer? topInfoContainer = playerState.GetNodeOrNull<HBoxContainer>("TopInfoContainer");
                    Control? officialStarContainer =
                        playerState.GetNodeOrNull<Control>("%StarCountContainer");
                    if (topInfoContainer == null || officialStarContainer == null)
                    {
                        continue;
                    }

                    Control? sunlightContainer = officialStarContainer.Duplicate() as Control;
                    if (sunlightContainer == null)
                    {
                        continue;
                    }

                    MegaLabel? sunlightLabel =
                        sunlightContainer.GetNodeOrNull<MegaLabel>("StarCount");
                    if (sunlightLabel == null)
                    {
                        sunlightContainer.QueueFree();
                        continue;
                    }

                    Texture2D? sunlightIcon =
                        GD.Load<Texture2D>("res://mods/KazamiYuka/images/sunlight.png");
                    if (sunlightIcon == null)
                    {
                        sunlightContainer.QueueFree();
                        continue;
                    }

                    sunlightContainer.Name = "SunlightCountContainer";
                    sunlightContainer.Visible = false;
                    sunlightContainer.MouseFilter = Control.MouseFilterEnum.Ignore;
                    ReplaceCounterIconTextures(sunlightContainer, sunlightIcon);
                    topInfoContainer.AddChild(sunlightContainer);

                    NMultiplayerSunlightCounter multiplayerCounter = new();
                    sunlightContainer.AddChild(multiplayerCounter);
                    multiplayerCounter.Initialize(playerState.Player, sunlightContainer, sunlightLabel);
                    _multiplayerPlayerCounters.Add(multiplayerCounter);
                }
            }
        }

        SunflowerManager.AttachTargetButtons(__instance);

        GD.Print("[KazamiYuka] 阳光计数器已按官方多人血条容器注入！");
    }

    [HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi.Deactivate))]
    public static class DeactivatePatch
    {
        public static void Postfix()
        {
            GD.Print("[InjectSunlightCounterPatch] Combat UI deactivated, clearing counter references");
            _energyCounter = null;
            _multiplayerPlayerCounters.Clear();
            SunflowerManager.DetachTargetButtons();
        }
    }

    private static void ReplaceCounterIconTextures(Node node, Texture2D sunlightIcon)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is TextureRect textureRect)
            {
                textureRect.Texture = sunlightIcon;
            }

            ReplaceCounterIconTextures(child, sunlightIcon);
        }
    }
}
