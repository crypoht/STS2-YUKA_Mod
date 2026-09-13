using Godot;
using HarmonyLib;
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
    private const string SunlightTexturePath = "res://mods/KazamiYuka/images/sunlight.png";

    [HarmonyPatch(nameof(NCard._Ready))]
    [HarmonyPostfix]
    public static void ReadyPostfix(NCard __instance)
    {
        if (__instance.Model is not KazamiYukaCard)
        {
            return;
        }

        CreateSunlightCostNode(__instance);
        UpdateSunlightCostNode(__instance);
    }

    [HarmonyPatch(nameof(NCard.UpdateVisuals))]
    [HarmonyPostfix]
    public static void UpdateVisualsPostfix(NCard __instance)
    {
        CreateSunlightCostNode(__instance);
        UpdateSunlightCostNode(__instance);
    }

    [HarmonyPatch("Reload")]
    [HarmonyPostfix]
    public static void ReloadPostfix(NCard __instance)
    {
        CreateSunlightCostNode(__instance);
        UpdateSunlightCostNode(__instance);
    }

    private static void CreateSunlightCostNode(NCard card)
    {
        if (card.Model is not KazamiYukaCard ||
            !card.Model.DynamicVars.ContainsKey("SunlightCost"))
        {
            return;
        }

        var cardContainer = card.GetNodeOrNull<Control>("%CardContainer");
        if (card.GetNodeOrNull<TextureRect>(SunlightIconName) != null ||
            cardContainer?.GetNodeOrNull<TextureRect>(SunlightIconName) != null)
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

        sunlightIcon.Name = SunlightIconName;
        sunlightIcon.UniqueNameInOwner = false;
        sunlightIcon.Position = energyIcon.Position + new Vector2(56f, -13f);
        sunlightIcon.Texture = PreloadManager.Cache.GetTexture2D(SunlightTexturePath);
        sunlightIcon.MouseFilter = Control.MouseFilterEnum.Ignore;
        sunlightIcon.TextureFilter = CanvasItem.TextureFilterEnum.Linear;
        sunlightIcon.Modulate = Colors.White;

        var starLabel = sunlightIcon.GetNodeOrNull<MegaLabel>("StarLabel");
        if (starLabel != null)
        {
            starLabel.Name = SunlightLabelName;
            starLabel.UniqueNameInOwner = false;
            starLabel.SelfModulate = Colors.White;
            starLabel.AddThemeColorOverride(ThemeConstants.Label.FontColor, Colors.White);
        }

        cardContainer.AddChild(sunlightIcon);
    }

    private static void UpdateSunlightCostNode(NCard card)
    {
        var cardContainer = card.GetNodeOrNull<Control>("%CardContainer");
        var sunlightIcon = cardContainer?.GetNodeOrNull<TextureRect>(SunlightIconName);
        if (sunlightIcon == null || card.Model == null)
        {
            return;
        }

        if (!card.Model.DynamicVars.TryGetValue("SunlightCost", out var costVar) || costVar == null)
        {
            sunlightIcon.Visible = false;
            return;
        }

        sunlightIcon.Visible = true;
        var sunlightLabel = sunlightIcon.GetNodeOrNull<MegaLabel>(SunlightLabelName);
        if (sunlightLabel != null)
        {
            sunlightLabel.SelfModulate = Colors.White;
            sunlightLabel.AddThemeColorOverride(ThemeConstants.Label.FontColor, Colors.White);
            sunlightLabel.SetTextAutoSize(((int)costVar.PreviewValue).ToString());
        }
    }
}
