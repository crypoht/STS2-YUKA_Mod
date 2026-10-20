using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using KazamiYuka.Scripts.Nodes;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi.Activate))]
public static class InjectSunlightCounterPatch
{
    private static NSunlightCounter? _energyCounter;
    private static readonly List<NMultiplayerSunlightCounter> MultiplayerCounters = new();

    public static void Postfix(NCombatUi __instance, CombatState? state)
    {
        GD.Print($"[KazamiYuka][CombatTrace] NCombatUi.Activate postfix entered. Players={state?.Players?.Count ?? -1}");
        if (state == null)
        {
            GD.PrintErr("[KazamiYuka][CombatTrace] Combat state is null; skipping combat UI initialization.");
            return;
        }

        try
        {
            GD.Print("[KazamiYuka][CombatTrace] ResetForCombat begin.");
            SunlightManager.ResetForCombat();
            GD.Print("[KazamiYuka][CombatTrace] ResetForCombat complete.");
            SunflowerManager.ResetCombatState();
            GD.Print("[KazamiYuka][CombatTrace] ResetCombatState complete.");
            _ = InitializeDeferred(__instance, state);
            GD.Print("[KazamiYuka][CombatTrace] Deferred combat UI initialization scheduled.");
        }
        catch (Exception exception)
        {
            GD.PrintErr($"[KazamiYuka][CombatTrace] Activate postfix failed: {exception}");
        }
    }

    private static async Task InitializeDeferred(NCombatUi combatUi, CombatState state)
    {
        try
        {
            GD.Print("[KazamiYuka][CombatTrace] Deferred initialization entered.");
            await combatUi.ToSignal(combatUi.GetTree(), SceneTree.SignalName.ProcessFrame);
            GD.Print("[KazamiYuka][CombatTrace] Deferred frame 1 complete.");
            await combatUi.ToSignal(combatUi.GetTree(), SceneTree.SignalName.ProcessFrame);
            GD.Print("[KazamiYuka][CombatTrace] Deferred frame 2 complete.");

            if (!GodotObject.IsInstanceValid(combatUi))
            {
                GD.PrintErr("[KazamiYuka][CombatTrace] Combat UI became invalid.");
                return;
            }

            Player? localPlayer = LocalContext.GetMe(state);
            GD.Print($"[KazamiYuka][CombatTrace] Local player resolved: {localPlayer != null}.");
            if (localPlayer == null ||
                (!SunlightManager.IsYukaPlayer(localPlayer) && state.Players.Count <= 1))
            {
                GD.Print("[KazamiYuka][CombatTrace] No sunlight UI required for this combat.");
                return;
            }

            GD.Print("[KazamiYuka][CombatTrace] Loading sunlight counter scene.");
            PackedScene? scene = GD.Load<PackedScene>(
                "res://mods/KazamiYuka/scenes/SunlightCounter.tscn");
            if (scene == null)
            {
                GD.PrintErr("[KazamiYuka] SunlightCounter scene not found.");
                return;
            }

            if (_energyCounter == null || !GodotObject.IsInstanceValid(_energyCounter))
            {
                NSunlightCounter? sunlightCounter = scene.Instantiate<NSunlightCounter>();
                if (sunlightCounter == null)
                {
                    return;
                }

                sunlightCounter.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
                sunlightCounter.Position = new Vector2(-35f, -40f);
                combatUi.EnergyCounterContainer.AddChild(sunlightCounter);
                _energyCounter = sunlightCounter;
                GD.Print("[KazamiYuka][CombatTrace] Main sunlight counter attached.");
            }

            if (state.Players.Count > 1)
            {
                GD.Print("[KazamiYuka][CombatTrace] Setting up multiplayer sunlight counters.");
                SetupMultiplayerCounters();
                GD.Print("[KazamiYuka][CombatTrace] Multiplayer sunlight counters complete.");
            }

            GD.Print("[KazamiYuka][CombatTrace] Attaching sunflower target buttons.");
            SunflowerManager.AttachTargetButtons(combatUi);
            GD.Print("[KazamiYuka][CombatTrace] Sunflower target buttons attached.");

            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.CombatEnded -= OnCombatEnded;
                CombatManager.Instance.CombatEnded += OnCombatEnded;
            }
            GD.Print("[KazamiYuka][CombatTrace] Deferred combat UI initialization complete.");
        }
        catch (Exception exception)
        {
            GD.PrintErr($"[KazamiYuka] Combat UI initialization failed: {exception}");
        }
    }

    private static void SetupMultiplayerCounters()
    {
        NMultiplayerPlayerStateContainer? playerStates =
            NRun.Instance?.GlobalUi?.MultiplayerPlayerContainer;
        if (playerStates == null)
        {
            return;
        }

        Texture2D? sunlightIcon =
            GD.Load<Texture2D>("res://mods/KazamiYuka/images/sunlight.png");
        if (sunlightIcon == null)
        {
            return;
        }

        foreach (Node child in playerStates.GetChildren())
        {
            if (child is not NMultiplayerPlayerState playerState)
            {
                continue;
            }

            HBoxContainer? topInfoContainer =
                playerState.GetNodeOrNull<HBoxContainer>("TopInfoContainer");
            Control? officialStarContainer =
                playerState.GetNodeOrNull<Control>("%StarCountContainer");
            if (topInfoContainer == null || officialStarContainer == null)
            {
                continue;
            }

            Control? sunlightContainer =
                topInfoContainer.GetNodeOrNull<Control>("SunlightCountContainer");
            if (sunlightContainer == null)
            {
                sunlightContainer = officialStarContainer.Duplicate() as Control;
                if (sunlightContainer == null)
                {
                    continue;
                }

                sunlightContainer.Name = "SunlightCountContainer";
                topInfoContainer.AddChild(sunlightContainer);
            }

            MegaLabel? sunlightLabel =
                sunlightContainer.GetNodeOrNull<MegaLabel>("StarCount");
            if (sunlightLabel == null)
            {
                continue;
            }

            sunlightContainer.Visible = false;
            sunlightContainer.MouseFilter = Control.MouseFilterEnum.Ignore;
            ReplaceCounterIconTextures(sunlightContainer, sunlightIcon);

            NMultiplayerSunlightCounter? counter = null;
            foreach (Node node in sunlightContainer.GetChildren())
            {
                if (node is NMultiplayerSunlightCounter existingCounter)
                {
                    counter = existingCounter;
                    break;
                }
            }

            counter ??= new NMultiplayerSunlightCounter();
            if (counter.GetParent() == null)
            {
                sunlightContainer.AddChild(counter);
            }

            counter.Initialize(playerState.Player, sunlightContainer, sunlightLabel);
            if (!MultiplayerCounters.Contains(counter))
            {
                MultiplayerCounters.Add(counter);
            }
        }
    }

    private static void OnCombatEnded(CombatRoom _)
    {
        SunflowerManager.DetachTargetButtons();
    }

    [HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi.Deactivate))]
    public static class DeactivatePatch
    {
        public static void Postfix()
        {
            if (_energyCounter != null && GodotObject.IsInstanceValid(_energyCounter))
            {
                _energyCounter.QueueFree();
            }

            foreach (NMultiplayerSunlightCounter counter in MultiplayerCounters)
            {
                if (GodotObject.IsInstanceValid(counter))
                {
                    counter.QueueFree();
                }
            }

            _energyCounter = null;
            MultiplayerCounters.Clear();
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.CombatEnded -= OnCombatEnded;
            }

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
