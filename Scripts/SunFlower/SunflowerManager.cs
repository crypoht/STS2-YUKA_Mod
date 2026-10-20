using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using KazamiYuka.Scripts.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace KazamiYuka.Scripts;

public enum FlowerHealthEvent
    {
        Heal,
        Damage,
        MaxHpIncrease,
        MaxHpDecrease
    }

    public delegate Task FlowerHealthChangedHandler(PlayerChoiceContext ctx, Creature sunflower, FlowerHealthEvent eventType, decimal amount);

public static class SunflowerManager
{
    private static readonly Dictionary<ulong, int> _relicXiangYangApplications = new();
    private static readonly Dictionary<ulong, Player> _relicPlayers = new();
    private static readonly List<TextureRect> _sunflowerTargetButtons = new();
    private static readonly List<Button> _sunflowerTargetHitboxes = new();
    private static readonly List<(Button Hitbox, NCreature FlowerNode)> _sunflowerTargetHitboxTargets = new();
    private static readonly List<NSelectionReticle> _sunflowerTargetReticles = new();
    private static Control? _sunflowerTargetPanel;
    private static readonly List<NHealthBar> _sunflowerHealthBars = new();
    private static readonly List<(Creature Flower, Action<int, int> CurrentHpChanged, Action<int, int> MaxHpChanged)> _sunflowerHealthSubscriptions = new();
    private static readonly List<Creature> _sunflowerSlots = new();
    private static readonly Dictionary<Creature, int> _sunflowerSlotByCreature = new();
    private static readonly Stack<int> _freeSunflowerSlots = new();
    private static int _nextSunflowerSlot;
    private static bool _sunflowerRearrangeQueued;
    private static Control? _sunflowerCombatClip;
    private static NTargetManager? _subscribedTargetManager;
    private static bool _isSunflowerTargetingCardActive;
    private static CardModel? _activeTargetingCard;
    private static bool _isRefreshingTargetButtons;
    private static bool _targetButtonsRefreshQueued;
    private static int _targetButtonsHideDepth;
    private const float SunflowerTargetPanelY = 430f;
    private const float SunflowerTargetRowHeight = 64f;
    private const float SunflowerAvatarOffsetX = 14f;
    private const float SunflowerAvatarOffsetY = 10f;
    private const float SunflowerAvatarSize = 48f;
    private const float SunflowerHealthBarOffsetY = 41f;
    private const float SunflowerHealthBarOffsetX = 63f;
    private const float SunflowerTargetReticleOffsetX = -8f;
    private const float SunflowerTargetReticleOffsetY = -6f;
    private const float SunflowerCombatClipTop = 0f;
    private const float SunflowerCombatClipBottom = 760f;

    public static List<Creature> GetSunflowers(Player player)
    {
        return player?.Creature?.CombatState?.Allies?
            .Where(c => c.IsAlive && c.Monster is KazamiYukaSunflower)
            .ToList() ?? new List<Creature>();
    }

    public static IReadOnlyList<Player> GetCombatPlayers(Creature creature)
    {
        return creature.CombatState?.Players ?? Array.Empty<Player>();
    }

    public static IReadOnlyList<Creature> GetAllCombatSunflowers(Creature creature)
    {
        return GetOrderedSunflowers()
            .Where(flower =>
                flower.IsAlive &&
                flower.Monster is KazamiYukaSunflower &&
                flower.CombatState == creature.CombatState)
            .ToList();
    }

    public static void ResetCombatState()
    {
        _relicXiangYangApplications.Clear();
        foreach (Creature sunflower in _sunflowerSlots)
        {
            sunflower.Died -= OnSunflowerDied;
        }

        _sunflowerSlots.Clear();
        _sunflowerSlotByCreature.Clear();
        _freeSunflowerSlots.Clear();
        _nextSunflowerSlot = 0;
        _sunflowerRearrangeQueued = false;
        RefreshRelicCounters();
    }

    public static int GetRelicXiangYangApplicationsRemaining(Player player)
    {
        if (player == null || player.Relics == null)
        {
            return 0;
        }

        bool hasSunHat = player.Relics.Any(relic => relic is KazamiYukaStarterRelic);
        bool hasOldSleepingCap = player.Relics.Any(relic => relic is Relics.OldSleepingCap_Yuka);
        if (!hasSunHat && !hasOldSleepingCap)
        {
            return 0;
        }

        _relicPlayers[player.NetId] = player;
        return Math.Max(0, 3 - _relicXiangYangApplications.GetValueOrDefault(player.NetId));
    }

    private static void RefreshRelicCounters()
    {
        foreach (Player player in _relicPlayers.Values)
        {
            RefreshRelicCounter(player);
        }
    }

    private static readonly Vector2[] SunflowerSlotOffsets =
    [
        new Vector2(0f, -110f),
        new Vector2(220f, -120f),
        new Vector2(440f, -105f),
        new Vector2(110f, 18f),
        new Vector2(330f, 4f)
    ];

    public static async Task Summon(Player summoner, decimal initialHp, int maxSunflowers = 5, bool grantGuard = false, bool grantDegraded = false)
    {
        if (summoner == null) return;

        var existingSunflowers = GetSunflowers(summoner);

        if (existingSunflowers.Count >= maxSunflowers)
        {
            decimal overflowMaxHp = initialHp / 2m;
            if (overflowMaxHp > 0)
            {
                await GainMaxHpAll(null!, summoner, overflowMaxHp);
            }
            return;
        }

        decimal surroundBonusMaxHp = SurroundPower.GetBonusMaxHp(summoner);
        decimal headpieceBonusMaxHp = Relics.SunflowerHeadpiece_Yuka.GetBonusMaxHp(summoner);
        if (summoner.Character is KazamiYukaCharacter)
        {
            await CreatureCmd.TriggerAnim(summoner.Creature, "Cast", summoner.Character.CastAnimDelay);
        }

        Creature newFlower = await PlayerCmd.AddPet<KazamiYukaSunflower>(summoner);
        RegisterSunflowerSlot(newFlower);

        if (grantDegraded)
        {
            await PowerCmd.Apply<DegradedPower>(
                new ThrowingPlayerChoiceContext(),
                newFlower,
                1m,
                summoner.Creature,
                null,
                false
            );
        }

        decimal newFlowerMaxHp = initialHp + surroundBonusMaxHp + headpieceBonusMaxHp;
        if (grantDegraded)
        {
            newFlowerMaxHp = Math.Min(newFlowerMaxHp, 1m);
        }

        await CreatureCmd.SetMaxHp(newFlower, newFlowerMaxHp);
        await CreatureCmd.Heal(newFlower, newFlowerMaxHp, true);

        ArrangeSunflowers(summoner);

        decimal relicXiangYangBonus = GetRelicXiangYangBonus(summoner);
        if (relicXiangYangBonus > 0m)
        {
            _relicXiangYangApplications[summoner.NetId] =
                _relicXiangYangApplications.GetValueOrDefault(summoner.NetId) + 1;
            RefreshRelicCounter(summoner);
        }

        if (CombatManager.Instance != null && CombatManager.Instance.IsInProgress)
        {
            SfxCmd.Play("event:/sfx/player/defect/orb_dark_evoke", 1f);
        }

        if (newFlower != null)
        {
            decimal xiangYangAmount = relicXiangYangBonus + DoubleHeadedSunflowerPower.GetExtraXiangYang(summoner);
            if (xiangYangAmount > 0m)
            {
                await PowerCmd.Apply<XiangYangPower>(
                    new ThrowingPlayerChoiceContext(),
                    newFlower,
                    xiangYangAmount,
                    summoner.Creature,
                    null
                );
            }

            if (grantGuard || surroundBonusMaxHp > 0)
            {
                await PowerCmd.Apply<SunflowerGuardPower>(
                    new ThrowingPlayerChoiceContext(),
                    newFlower,
                    1m,
                    summoner.Creature,
                    null,
                    false
                );
            }

        }
    }

    private static decimal GetRelicXiangYangBonus(Player summoner)
    {
        if (summoner.Relics == null ||
            _relicXiangYangApplications.GetValueOrDefault(summoner.NetId) >= 3)
        {
            return 0m;
        }

        foreach (var relic in summoner.Relics)
        {
            if (relic is Relics.OldSleepingCap_Yuka)
            {
                return 3m;
            }
        }

        foreach (var relic in summoner.Relics)
        {
            if (relic is KazamiYukaStarterRelic)
            {
                return 2m;
            }
        }

        return 0m;
    }

    private static void RefreshRelicCounter(Player player)
    {
        foreach (var relic in player.Relics)
        {
            if (relic is KazamiYukaStarterRelic starterRelic)
            {
                starterRelic.RefreshSunflowerCounter();
            }
            else if (relic is Relics.OldSleepingCap_Yuka oldSleepingCap)
            {
                oldSleepingCap.RefreshSunflowerCounter();
            }
        }
    }
    public static void ArrangeSunflowers(Player summoner)
    {
        if (!IsCombatActivelyRunning())
        {
            return;
        }

        ArrangeSunflowers(
            summoner,
            GetOrderedSunflowers().Where(flower => flower.PetOwner == summoner && flower.IsAlive).ToList());
        RefreshTargetButtons();
    }

    public static async void ArrangeSunflowersDeferred(Player summoner)
    {
        if (!IsCombatActivelyRunning())
        {
            return;
        }

        NCombatRoom? room = NCombatRoom.Instance;
        if (room == null)
        {
            return;
        }

        await room.ToSignal(room.GetTree(), SceneTree.SignalName.ProcessFrame);

        if (!IsCombatActivelyRunning())
        {
            return;
        }

        ArrangeSunflowers(summoner);
    }

    private static void ArrangeSunflowers(Player summoner, IReadOnlyList<Creature> sunflowers)
    {
        if (!IsCombatActivelyRunning())
        {
            return;
        }

        NCombatRoom? room = NCombatRoom.Instance;
        if (room == null)
        {
            return;
        }

        Player localPlayer = summoner.Creature.CombatState == null
            ? summoner
            : LocalContext.GetMe(summoner.Creature.CombatState) ?? summoner;
        NCreature? playerNode = room.GetCreatureNode(localPlayer.Creature);
        if (playerNode == null)
        {
            return;
        }

        Vector2 playerRelativeOrigin = playerNode.Position + new Vector2(
            playerNode.Hitbox.Size.X * 0.5f + 90f,
            20f
        );

        foreach (Creature flower in sunflowers)
        {
            NCreature? flowerNode = room.GetCreatureNode(flower);
            if (flowerNode == null)
            {
                continue;
            }

            // 使用向日葵固定槽号定位；不按当前存活数量重新编号。
            int slotIndex = _sunflowerSlotByCreature.GetValueOrDefault(flower);
            Vector2 offset = SunflowerSlotOffsets[slotIndex % SunflowerSlotOffsets.Length];
            flowerNode.Position = playerRelativeOrigin + offset;
            SetSunflowerLayer(flowerNode, playerNode, slotIndex);
            room.SetCreatureIsInteractable(flower, true);

            if (flowerNode.Visuals?.Bounds != null)
            {
                flowerNode.Visuals.Bounds.Visible = true;
            }

        }
    }

    private static bool IsCombatActivelyRunning()
    {
        CombatManager? combatManager = CombatManager.Instance;
        return combatManager is { IsInProgress: true, IsOverOrEnding: false };
    }

    private static void SetSunflowerLayer(NCreature flowerNode, NCreature playerNode, int slotIndex)
    {
        flowerNode.ZAsRelative = true;
        flowerNode.ZIndex = 1;
        flowerNode.Visible = true;

        if (flowerNode.Visuals != null)
        {
            flowerNode.Visuals.ZAsRelative = true;
            flowerNode.Visuals.ZIndex = 0;
            flowerNode.Visuals.Visible = true;
        }
    }

    public static void AttachTargetButtons(NCombatUi combatUi)
    {
        DetachTargetButtons();
        SubscribeTargetingSignals();

        Control targetParent = (Control?)NRun.Instance?.GlobalUi ?? combatUi;
        int targetLayer = NRun.Instance?.GlobalUi?.MultiplayerPlayerContainer?.ZIndex ?? 0;
        _sunflowerCombatClip = new Control
        {
            Name = "SunflowerCombatClip",
            Position = new Vector2(0f, SunflowerCombatClipTop),
            Size = new Vector2(1920f, SunflowerCombatClipBottom - SunflowerCombatClipTop),
            ClipContents = true,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ZAsRelative = true,
            ZIndex = targetLayer
        };
        targetParent.AddChild(_sunflowerCombatClip);

        _sunflowerTargetPanel = new Control
        {
            Name = "SunflowerTargetPanel",
            Position = new Vector2(18f, SunflowerTargetPanelY),
            Size = new Vector2(280f, 360f),
            ClipContents = true,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ZAsRelative = true,
            ZIndex = 0
        };
        _sunflowerCombatClip.AddChild(_sunflowerTargetPanel);
        RefreshTargetButtons();
    }

    public static void DetachTargetButtons()
    {
        foreach (TextureRect button in _sunflowerTargetButtons)
        {
            if (GodotObject.IsInstanceValid(button))
            {
                button.QueueFree();
            }
        }

        _sunflowerTargetButtons.Clear();
        foreach (Button hitbox in _sunflowerTargetHitboxes)
        {
            if (GodotObject.IsInstanceValid(hitbox))
            {
                hitbox.QueueFree();
            }
        }

        _sunflowerTargetHitboxes.Clear();
        _sunflowerTargetHitboxTargets.Clear();
        foreach (NSelectionReticle reticle in _sunflowerTargetReticles)
        {
            if (GodotObject.IsInstanceValid(reticle))
            {
                reticle.QueueFree();
            }
        }

        _sunflowerTargetReticles.Clear();
        foreach (NHealthBar healthBar in _sunflowerHealthBars)
        {
            if (GodotObject.IsInstanceValid(healthBar))
            {
                healthBar.QueueFree();
            }
        }

        _sunflowerHealthBars.Clear();
        foreach (var subscription in _sunflowerHealthSubscriptions)
        {
            subscription.Flower.CurrentHpChanged -= subscription.CurrentHpChanged;
            subscription.Flower.MaxHpChanged -= subscription.MaxHpChanged;
        }

        _sunflowerHealthSubscriptions.Clear();
        if (_sunflowerTargetPanel != null && GodotObject.IsInstanceValid(_sunflowerTargetPanel))
        {
            _sunflowerTargetPanel.QueueFree();
        }

        _sunflowerTargetPanel = null;
        if (_sunflowerCombatClip != null && GodotObject.IsInstanceValid(_sunflowerCombatClip))
        {
            _sunflowerCombatClip.QueueFree();
        }

        _sunflowerCombatClip = null;
        _isSunflowerTargetingCardActive = false;
        _activeTargetingCard = null;
        _targetButtonsHideDepth = 0;
    }

    public static void SetActiveTargetingCard(CardModel? card)
    {
        _activeTargetingCard = card;
        _isSunflowerTargetingCardActive =
            card != null &&
            (card.TargetType == KazamiYukaTargetTypes.Sunflower ||
             SqueezeManager.IsSqueezed(card)) &&
            (card is ICustomTargetValidator || SqueezeManager.IsSqueezed(card));
        RefreshSunflowerTargetHitboxes();
    }

    public static CardModel? GetActiveTargetingCard() => _activeTargetingCard;

    public static void HideTargetButtons()
    {
        if (_sunflowerCombatClip != null && GodotObject.IsInstanceValid(_sunflowerCombatClip))
        {
            _sunflowerCombatClip.Visible = false;
        }

        DisableSunflowerTargetHitboxes();
    }

    public static void ShowTargetButtons()
    {
        if (_targetButtonsHideDepth > 0)
        {
            return;
        }

        if (_sunflowerCombatClip != null && GodotObject.IsInstanceValid(_sunflowerCombatClip))
        {
            _sunflowerCombatClip.Visible = true;
        }
    }

    public static void PushTargetButtonsHidden()
    {
        _targetButtonsHideDepth++;
        HideTargetButtons();
    }

    public static void PopTargetButtonsHidden()
    {
        if (_targetButtonsHideDepth > 0)
        {
            _targetButtonsHideDepth--;
        }

        ShowTargetButtons();
    }

    public static void RefreshTargetButtons()
    {
        if (_sunflowerTargetPanel == null || !GodotObject.IsInstanceValid(_sunflowerTargetPanel))
        {
            return;
        }

        if (_isRefreshingTargetButtons)
        {
            QueueTargetButtonsRefresh();
            return;
        }

        _isRefreshingTargetButtons = true;
        try
        {
            foreach (TextureRect button in _sunflowerTargetButtons)
            {
                if (GodotObject.IsInstanceValid(button))
                {
                    button.QueueFree();
                }
            }

            _sunflowerTargetButtons.Clear();
            foreach (Button hitbox in _sunflowerTargetHitboxes)
            {
                if (GodotObject.IsInstanceValid(hitbox))
                {
                    hitbox.QueueFree();
                }
            }

            _sunflowerTargetHitboxes.Clear();
            _sunflowerTargetHitboxTargets.Clear();
            foreach (NSelectionReticle reticle in _sunflowerTargetReticles)
            {
                if (GodotObject.IsInstanceValid(reticle))
                {
                    reticle.QueueFree();
                }
            }

            _sunflowerTargetReticles.Clear();
            foreach (NHealthBar healthBar in _sunflowerHealthBars)
            {
                if (GodotObject.IsInstanceValid(healthBar))
                {
                    healthBar.QueueFree();
                }
            }

            _sunflowerHealthBars.Clear();
            foreach (var subscription in _sunflowerHealthSubscriptions)
            {
                subscription.Flower.CurrentHpChanged -= subscription.CurrentHpChanged;
                subscription.Flower.MaxHpChanged -= subscription.MaxHpChanged;
            }

            _sunflowerHealthSubscriptions.Clear();
            NCombatRoom? room = NCombatRoom.Instance;
            if (room == null)
            {
                return;
            }

            int displayIndex = 0;
            foreach (Creature flower in GetOrderedSunflowers())
            {
                if (flower.Monster is not KazamiYukaSunflower || !flower.IsAlive)
                {
                    continue;
                }

                NCreature? flowerNode = room.GetCreatureNode(flower);
                if (flowerNode == null)
                {
                    continue;
                }

                TextureRect? button = null;
                try
                {
                    PackedScene? iconScene = PreloadManager.Cache.GetScene(
                        "res://mods/KazamiYuka/scenes/Sunflower_icon.tscn");
                    button = iconScene?.Instantiate<TextureRect>();
                }
                catch (Exception exception)
                {
                    GD.PrintErr($"[KazamiYuka] Failed to load Sunflower_icon.tscn: {exception.Message}");
                }

                if (button == null)
                {
                    Texture2D? iconTexture = GD.Load<Texture2D>(
                        "res://mods/KazamiYuka/images/SunFlower_icon.png");
                    if (iconTexture != null)
                    {
                        button = new TextureRect
                        {
                            Texture = iconTexture
                        };
                    }
                }

                if (button == null)
                {
                    continue;
                }

                int rowIndex = displayIndex;
                button.Name = $"SunflowerTargetButton{rowIndex}";
                button.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
                button.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                button.Position = new Vector2(
                    SunflowerAvatarOffsetX,
                    rowIndex * SunflowerTargetRowHeight + SunflowerAvatarOffsetY);
                button.Size = new Vector2(SunflowerAvatarSize, SunflowerAvatarSize);
                button.MouseFilter = Control.MouseFilterEnum.Ignore;
                button.ZAsRelative = true;
                button.ZIndex = 1;
                _sunflowerTargetPanel.AddChild(button);
                _sunflowerTargetButtons.Add(button);

                Button targetHitbox = new()
                {
                    Name = $"SunflowerTargetHitbox{rowIndex}",
                    Position = new Vector2(0f, rowIndex * SunflowerTargetRowHeight),
                    Size = new Vector2(280f, SunflowerTargetRowHeight),
                    Text = string.Empty,
                    Flat = true,
                    FocusMode = Control.FocusModeEnum.None,
                    MouseFilter = Control.MouseFilterEnum.Ignore,
                    ZAsRelative = true,
                    ZIndex = 3
                };
                targetHitbox.MouseEntered += () => HoverSunflowerTarget(flowerNode);
                targetHitbox.MouseExited += () => UnhoverSunflowerTarget(flowerNode);
                targetHitbox.Pressed += () => SelectSunflowerTarget(flowerNode);
                _sunflowerTargetPanel.AddChild(targetHitbox);
                _sunflowerTargetHitboxes.Add(targetHitbox);
                _sunflowerTargetHitboxTargets.Add((targetHitbox, flowerNode));

                NCreatureStateDisplay? stateDisplay = flowerNode.GetNodeOrNull<NCreatureStateDisplay>("%HealthBar");
                NHealthBar? sourceHealthBar = stateDisplay?.GetNodeOrNull<NHealthBar>("%HealthBar");
                if (sourceHealthBar == null)
                {
                    continue;
                }

                NHealthBar? healthBar = sourceHealthBar.Duplicate() as NHealthBar;
                if (healthBar == null)
                {
                    continue;
                }

                healthBar.Name = $"SunflowerHealthBar{rowIndex}";
                healthBar.Position = new Vector2(
                    SunflowerHealthBarOffsetX,
                    rowIndex * SunflowerTargetRowHeight + SunflowerHealthBarOffsetY);
                healthBar.Scale = Vector2.One;
                healthBar.MouseFilter = Control.MouseFilterEnum.Ignore;
                healthBar.ZAsRelative = true;
                healthBar.ZIndex = 2;
                healthBar.Visible = true;
                _sunflowerTargetPanel.AddChild(healthBar);
                healthBar.SetCreature(flower);
                healthBar.HpBarContainer.Position = Vector2.Zero;
                healthBar.HpBarContainer.Size = new Vector2(173f, healthBar.HpBarContainer.Size.Y);
                healthBar.RefreshValues();
                _sunflowerHealthBars.Add(healthBar);

                NSelectionReticle targetReticle = PreloadManager.Cache.GetScene(
                    "res://scenes/ui/selection_reticle.tscn").Instantiate<NSelectionReticle>();
                targetReticle.Name = $"SunflowerTargetReticle{rowIndex}";
                targetReticle.Position = healthBar.Position + new Vector2(
                    SunflowerTargetReticleOffsetX,
                    SunflowerTargetReticleOffsetY);
                targetReticle.Size = new Vector2(190f, 44f);
                targetReticle.MouseFilter = Control.MouseFilterEnum.Ignore;
                targetReticle.ZAsRelative = true;
                targetReticle.ZIndex = 4;
                targetReticle.Visible = true;
                _sunflowerTargetPanel.AddChild(targetReticle);
                _sunflowerTargetReticles.Add(targetReticle);

                targetHitbox.ZIndex = 5;
                targetHitbox.MoveToFront();

                Action<int, int> currentHpChanged = (_, _) => QueueTargetButtonsRefresh();
                Action<int, int> maxHpChanged = (_, _) => QueueTargetButtonsRefresh();
                flower.CurrentHpChanged += currentHpChanged;
                flower.MaxHpChanged += maxHpChanged;
                _sunflowerHealthSubscriptions.Add((flower, currentHpChanged, maxHpChanged));
                targetHitbox.MouseEntered += () => SetSunflowerTargetReticleVisible(targetReticle, true);
                targetHitbox.MouseExited += () => SetSunflowerTargetReticleVisible(targetReticle, false);

                displayIndex++;
            }

            RefreshSunflowerTargetHitboxes();
        }
        finally
        {
            _isRefreshingTargetButtons = false;
        }
    }

    private static async void QueueTargetButtonsRefresh()
    {
        if (_targetButtonsRefreshQueued)
        {
            return;
        }

        _targetButtonsRefreshQueued = true;
        try
        {
            NCombatRoom? room = NCombatRoom.Instance;
            if (room == null)
            {
                return;
            }

            await room.ToSignal(room.GetTree(), SceneTree.SignalName.ProcessFrame);
            RefreshTargetButtons();
        }
        finally
        {
            _targetButtonsRefreshQueued = false;
        }
    }

    private static void RegisterSunflowerSlot(Creature sunflower)
    {
        if (sunflower.Monster is not KazamiYukaSunflower ||
            !sunflower.IsAlive ||
            _sunflowerSlotByCreature.ContainsKey(sunflower))
        {
            return;
        }

        Player? owner = sunflower.PetOwner;
        if (owner == null)
        {
            return;
        }

        // 优先复用最近死亡向日葵释放的槽位，否则分配下一个新槽位。
        int slot;
        if (_freeSunflowerSlots.Count > 0)
        {
            slot = _freeSunflowerSlots.Pop();
        }
        else
        {
            slot = _nextSunflowerSlot++;
        }

        _sunflowerSlotByCreature[sunflower] = slot;
        _sunflowerSlots.Add(sunflower);
        sunflower.Died += OnSunflowerDied;
    }

    private static void OnSunflowerDied(Creature sunflower)
    {
        if (!_sunflowerSlotByCreature.Remove(sunflower, out int slot))
        {
            return;
        }

        // 死亡只释放原槽位，不移动存活向日葵，也不压缩它们的编号。
        _freeSunflowerSlots.Push(slot);
        _sunflowerSlots.Remove(sunflower);
        sunflower.Died -= OnSunflowerDied;
        QueueSunflowerRearrange();
    }

    public static void ArrangeAllSunflowersDeferred()
    {
        QueueSunflowerRearrange();
    }

    private static async void QueueSunflowerRearrange()
    {
        if (_sunflowerRearrangeQueued ||
            CombatManager.Instance is not { IsInProgress: true, IsOverOrEnding: false } ||
            NCombatRoom.Instance is not NCombatRoom room)
        {
            return;
        }

        _sunflowerRearrangeQueued = true;
        try
        {
            // 官方死亡流程可能还会继续修改宠物节点；等待一帧后再恢复固定槽位。
            await room.ToSignal(room.GetTree(), SceneTree.SignalName.ProcessFrame);
            if (CombatManager.Instance is not { IsInProgress: true, IsOverOrEnding: false })
            {
                return;
            }

            foreach (Player owner in _sunflowerSlots
                         .Select(flower => flower.PetOwner)
                         .OfType<Player>()
                         .Distinct())
            {
                ArrangeSunflowers(owner);
            }
        }
        finally
        {
            _sunflowerRearrangeQueued = false;
        }
    }

    private static IReadOnlyList<Creature> GetOrderedSunflowers()
    {
        NCombatRoom? room = NCombatRoom.Instance;
        if (room != null)
        {
            foreach (NCreature creatureNode in room.CreatureNodes)
            {
                Creature? creature = creatureNode.Entity;
                if (creature is { IsAlive: true } && creature.Monster is KazamiYukaSunflower)
                {
                    RegisterSunflowerSlot(creature);
                }
            }
        }

        return _sunflowerSlots
            .OrderBy(flower => _sunflowerSlotByCreature.GetValueOrDefault(flower))
            .ToList();
    }

    private static void SelectSunflowerTarget(NCreature flowerNode)
    {
        NTargetManager? targetManager = NTargetManager.Instance;
        if (targetManager == null
            || !targetManager.IsInSelection
            || !GodotObject.IsInstanceValid(flowerNode)
            || !targetManager.AllowedToTargetNode(flowerNode))
        {
            return;
        }

        targetManager.OnNodeHovered(flowerNode);

        MethodInfo? finishTargeting = typeof(NTargetManager).GetMethod(
            "FinishTargeting",
            BindingFlags.Instance | BindingFlags.NonPublic);
        finishTargeting?.Invoke(targetManager, [false]);
    }

    private static void HoverSunflowerTarget(NCreature flowerNode)
    {
        NTargetManager? targetManager = NTargetManager.Instance;
        if (targetManager == null || !targetManager.IsInSelection || !GodotObject.IsInstanceValid(flowerNode))
        {
            return;
        }

        targetManager.OnNodeHovered(flowerNode);
    }

    private static void UnhoverSunflowerTarget(NCreature flowerNode)
    {
        NTargetManager? targetManager = NTargetManager.Instance;
        if (targetManager == null || !targetManager.IsInSelection || !GodotObject.IsInstanceValid(flowerNode))
        {
            return;
        }

        targetManager.OnNodeUnhovered(flowerNode);
    }

    private static void SubscribeTargetingSignals()
    {
        NTargetManager? targetManager = NTargetManager.Instance;
        if (targetManager == null || ReferenceEquals(_subscribedTargetManager, targetManager))
        {
            return;
        }

        if (_subscribedTargetManager != null && GodotObject.IsInstanceValid(_subscribedTargetManager))
        {
            _subscribedTargetManager.TargetingBegan -= RefreshSunflowerTargetHitboxes;
            _subscribedTargetManager.TargetingEnded -= DisableSunflowerTargetHitboxes;
        }

        _subscribedTargetManager = targetManager;
        targetManager.TargetingBegan += RefreshSunflowerTargetHitboxes;
        targetManager.TargetingEnded += DisableSunflowerTargetHitboxes;
    }

    private static void RefreshSunflowerTargetHitboxes()
    {
        NTargetManager? targetManager = NTargetManager.Instance;
        bool isSelecting = targetManager?.IsInSelection == true && _isSunflowerTargetingCardActive;

        foreach ((Button hitbox, NCreature flowerNode) in _sunflowerTargetHitboxTargets)
        {
            if (!GodotObject.IsInstanceValid(hitbox) || !GodotObject.IsInstanceValid(flowerNode))
            {
                continue;
            }

            bool canTargetSunflower = isSelecting && targetManager!.AllowedToTargetNode(flowerNode);
            hitbox.MouseFilter = canTargetSunflower
                ? Control.MouseFilterEnum.Stop
                : Control.MouseFilterEnum.Ignore;
        }
    }

    private static void DisableSunflowerTargetHitboxes()
    {
        _isSunflowerTargetingCardActive = false;
        _activeTargetingCard = null;
        foreach (Button hitbox in _sunflowerTargetHitboxes)
        {
            if (GodotObject.IsInstanceValid(hitbox))
            {
                hitbox.MouseFilter = Control.MouseFilterEnum.Ignore;
            }
        }
    }

    private static void SetSunflowerTargetReticleVisible(NSelectionReticle reticle, bool visible)
    {
        if (!GodotObject.IsInstanceValid(reticle))
        {
            return;
        }

        if (visible)
        {
            reticle.OnSelect();
        }
        else
        {
            reticle.OnDeselect();
        }
    }

    private static readonly List<FlowerHealthChangedHandler> _healthListeners = new();

    public static void RegisterHealthListener(FlowerHealthChangedHandler handler)
    {
        if (!_healthListeners.Contains(handler))
        {
            _healthListeners.Add(handler);
        }
    }

    private static async Task NotifyHealthChanged(PlayerChoiceContext ctx, Creature sunflower, FlowerHealthEvent eventType, decimal amount)
    {
        foreach (var listener in _healthListeners)
        {
            await listener(ctx, sunflower, eventType, amount);
        }

        QueueTargetButtonsRefresh();
    }

    public static Task NotifyDamage(PlayerChoiceContext ctx, Creature sunflower, decimal amount)
    {
        return NotifyHealthChanged(ctx, sunflower, FlowerHealthEvent.Damage, amount);
    }

    public static Task NotifyMaxHpDecrease(PlayerChoiceContext ctx, Creature sunflower, decimal amount)
    {
        return NotifyHealthChanged(ctx, sunflower, FlowerHealthEvent.MaxHpDecrease, amount);
    }

    public static async Task HealSingle(PlayerChoiceContext choiceContext, Creature sunflower, decimal amount)
    {
        if (sunflower != null && sunflower.IsAlive)
        {
            await CreatureCmd.Heal(sunflower, amount, true);
            await NotifyHealthChanged(choiceContext, sunflower, FlowerHealthEvent.Heal, amount);
        }
    }

    public static async Task HealAll(PlayerChoiceContext choiceContext, Player summoner, decimal amount)
    {
        foreach (var flower in GetSunflowers(summoner))
        {
            await HealSingle(choiceContext, flower, amount);
        }
    }

    public static async Task GainMaxHpSingle(PlayerChoiceContext choiceContext, Creature sunflower, decimal amount)
    {
        if (sunflower != null && sunflower.IsAlive)
        {
            decimal totalAmount = amount + GetFlourishBonus(sunflower);

            if (sunflower.GetPowerAmount<DegradedPower>() > 0)
            {
                totalAmount = Math.Min(totalAmount, Math.Max(0m, 1m - sunflower.MaxHp));
            }

            if (totalAmount > 0m)
            {
                await CreatureCmd.GainMaxHp(sunflower, totalAmount);
                await NotifyHealthChanged(choiceContext, sunflower, FlowerHealthEvent.MaxHpIncrease, totalAmount);
            }
        }
    }

    public static void UnregisterHealthListener(FlowerHealthChangedHandler handler)
    {
        _healthListeners.Remove(handler);
    }

    public static async Task GainMaxHpAll(PlayerChoiceContext choiceContext, Player summoner, decimal amount)
    {
        foreach (var flower in GetSunflowers(summoner))
        {
            await GainMaxHpSingle(choiceContext, flower, amount);
        }
    }

    private static decimal GetFlourishBonus(Creature sunflower)
    {
        return GetCombatPlayers(sunflower)
            .Select(player => player.Creature.GetPower<FlourishPower>())
            .Where(power => power != null)
            .Sum(power => power!.Amount);
    }
}
