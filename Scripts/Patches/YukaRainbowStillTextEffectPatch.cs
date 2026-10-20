using HarmonyLib;
using KazamiYuka.Scripts.RichTextTags;
using MegaCrit.Sts2.Core.RichTextTags;
using MegaCrit.Sts2.addons.mega_text;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(MegaRichTextLabel), "InstallEffectsIfNeeded")]
public static class YukaRainbowStillTextEffectInstallPatch
{
    public static void Postfix(MegaRichTextLabel __instance)
    {
        YukaRainbowStillTextEffectInstaller.EnsureInstalled(__instance);
    }
}

[HarmonyPatch(typeof(MegaRichTextLabel), "SetTextAutoSize")]
public static class YukaRainbowStillTextEffectSetTextPatch
{
    public static void Postfix(MegaRichTextLabel __instance)
    {
        YukaRainbowStillTextEffectInstaller.EnsureInstalled(__instance);

        if (__instance.BbcodeEnabled)
        {
            __instance.ParseBbcode(__instance.Text);
        }
    }
}

internal static class YukaRainbowStillTextEffectInstaller
{
    public static void EnsureInstalled(MegaRichTextLabel label)
    {
        if (!label.BbcodeEnabled)
        {
            return;
        }

        foreach (AbstractMegaRichTextEffect effect in label.CustomEffects)
        {
            if (effect is YukaRainbowStillTextEffect)
            {
                return;
            }
        }

        label.CustomEffects.Add(new YukaRainbowStillTextEffect());
    }
}
