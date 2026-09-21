using HarmonyLib;
using Godot;
using MegaCrit.Sts2.addons.mega_text;

namespace KazamiYuka.Scripts.Patches
{

    [HarmonyPatch(typeof(MegaRichTextLabel), "SetTextAutoSize")]
    public static class CustomTextIconRenderPatch
    {
        public static void Prefix(ref string text)
        {
            CustomTextIconRenderHelpers.ReplaceSunlightIcons(ref text);
        }
    }

    public static class CustomTextIconRenderHelpers
    {
        public static void ReplaceSunlightIcons(ref string text)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains("[sunlight]")) return;

            text = text.Replace(
                "[sunlight]",
                "[img width=16 height=16]res://mods/KazamiYuka/images/sunlight_icon.png[/img]");
        }
    }
}
