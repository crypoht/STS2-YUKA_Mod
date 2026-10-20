using Godot;
using HarmonyLib;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.addons.mega_text;
using KazamiYuka.Scripts.Cards;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(NCard))]
public static class CardSunlightCostPatch
{
    private const string SunlightIconName = "CustomSunlightIcon";
    private const string SunlightLabelName = "CustomSunlightLabel";
    private const string SunlightYIconName = "CustomSunlightYIcon";
    private const string SunlightYLabelName = "CustomSunlightYLabel";
    private const string SunlightTexturePath = "res://mods/KazamiYuka/images/sunlight.png";

    [HarmonyPatch(nameof(NCard._Ready))]
    [HarmonyPostfix]
    public static void ReadyPostfix(NCard __instance)
    {
        ScheduleRefresh(__instance);
    }

    [HarmonyPatch(nameof(NCard.OnInstantiated))]
    [HarmonyPostfix]
    public static void OnInstantiatedPostfix(NCard __instance)
    {
        ScheduleRefresh(__instance);
    }

    [HarmonyPatch(nameof(NCard.UpdateVisuals))]
    [HarmonyPostfix]
    public static void UpdateVisualsPostfix(NCard __instance)
    {
        Refresh(__instance);
    }

    [HarmonyPatch("Reload")]
    [HarmonyPostfix]
    public static void ReloadPostfix(NCard __instance)
    {
        ScheduleRefresh(__instance);
    }

    private static void ScheduleRefresh(NCard card)
    {
        Refresh(card);
        _ = RefreshNextFrame(card);
    }

    private static async Task RefreshNextFrame(NCard card)
    {
        if (!GodotObject.IsInstanceValid(card) || !card.IsInsideTree())
        {
            return;
        }

        await card.ToSignal(card.GetTree(), SceneTree.SignalName.ProcessFrame);
        if (GodotObject.IsInstanceValid(card))
        {
            Refresh(card);
        }
    }

    private static void Refresh(NCard card)
    {
        HideSunlightCostNodes(card);

        if (card.Model is not KazamiYukaCard)
        {
            return;
        }

        CreateSunlightCostNode(card);
        UpdateSunlightCostNode(card);
    }

    private static void HideSunlightCostNodes(NCard card)
    {
        var cardContainer = GetCardContainer(card);
        cardContainer?.GetNodeOrNull<TextureRect>(SunlightIconName)?.Hide();
        cardContainer?.GetNodeOrNull<TextureRect>(SunlightYIconName)?.Hide();
    }

    private static void CreateSunlightCostNode(NCard card)
    {
        if (card.Model is not KazamiYukaCard)
        {
            return;
        }

        bool hasFixedSunlightCost = card.Model.DynamicVars.ContainsKey("SunlightCost");
        bool hasSunlightY = card.Model.DynamicVars.ContainsKey("SunlightY");
        if (!hasFixedSunlightCost && !hasSunlightY)
        {
            return;
        }

        EnsureSunlightCostNode(card, hasSunlightY);
    }

    private static void EnsureSunlightCostNode(NCard card, bool isSunlightY)
    {
        var cardContainer = GetCardContainer(card);
        string iconName = isSunlightY ? SunlightYIconName : SunlightIconName;
        string labelName = isSunlightY ? SunlightYLabelName : SunlightLabelName;
        if (cardContainer?.GetNodeOrNull<TextureRect>(iconName) != null)
        {
            return;
        }

        var energyIcon = card.GetNodeOrNull<TextureRect>("%EnergyIcon");
        var starIcon = card.GetNodeOrNull<TextureRect>("%StarIcon");
        if (energyIcon == null || starIcon == null || cardContainer == null)
        {
            return;
        }

        var sunlightIcon = starIcon.Duplicate() as TextureRect;
        if (sunlightIcon == null)
        {
            return;
        }

        sunlightIcon.Name = iconName;
        sunlightIcon.UniqueNameInOwner = false;
        sunlightIcon.Position = energyIcon.Position + new Vector2(56f, -13f);
        sunlightIcon.Texture = PreloadManager.Cache.GetTexture2D(SunlightTexturePath);
        sunlightIcon.MouseFilter = Control.MouseFilterEnum.Ignore;
        sunlightIcon.TextureFilter = CanvasItem.TextureFilterEnum.Linear;
        sunlightIcon.Modulate = Colors.White;

        var starLabel = sunlightIcon.GetNodeOrNull<MegaLabel>("StarLabel");
        if (starLabel != null)
        {
            starLabel.Name = labelName;
            starLabel.UniqueNameInOwner = false;
            starLabel.SelfModulate = Colors.White;
            starLabel.AddThemeColorOverride(ThemeConstants.Label.FontColor, Colors.White);
        }

        cardContainer.AddChild(sunlightIcon);
    }

    private static void UpdateSunlightCostNode(NCard card)
    {
        var cardContainer = GetCardContainer(card);
        if (cardContainer == null || card.Model == null)
        {
            return;
        }

        bool hasSunlightY = card.Model.DynamicVars.ContainsKey("SunlightY");
        var fixedSunlightIcon = cardContainer.GetNodeOrNull<TextureRect>(SunlightIconName);
        var sunlightYIcon = cardContainer.GetNodeOrNull<TextureRect>(SunlightYIconName);
        if (fixedSunlightIcon != null)
        {
            fixedSunlightIcon.Visible = !hasSunlightY;
        }

        if (sunlightYIcon != null)
        {
            sunlightYIcon.Visible = hasSunlightY;
        }

        var sunlightIcon = hasSunlightY ? sunlightYIcon : fixedSunlightIcon;
        if (sunlightIcon == null)
        {
            return;
        }

        string labelName = hasSunlightY ? SunlightYLabelName : SunlightLabelName;
        var sunlightLabel = sunlightIcon.GetNodeOrNull<MegaLabel>(labelName);
        if (sunlightLabel == null)
        {
            return;
        }

        sunlightIcon.Visible = true;
        sunlightLabel.SelfModulate = Colors.White;
        sunlightLabel.AddThemeColorOverride(ThemeConstants.Label.FontColor, Colors.White);
        if (hasSunlightY)
        {
            sunlightLabel.SetTextAutoSize("Y");
            return;
        }

        if (!card.Model.DynamicVars.TryGetValue("SunlightCost", out var costVar) || costVar == null)
        {
            sunlightIcon.Visible = false;
            return;
        }

        sunlightLabel.SetTextAutoSize(((int)costVar.PreviewValue).ToString());
    }

    private static Control? GetCardContainer(NCard card) =>
        card.GetNodeOrNull<Control>("%CardContainer") ?? card.Body;
}
