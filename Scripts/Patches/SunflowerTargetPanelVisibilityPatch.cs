using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

namespace KazamiYuka.Scripts.Patches;

[HarmonyPatch(typeof(NCapstoneSubmenuStack), nameof(NCapstoneSubmenuStack.AfterCapstoneOpened))]
public static class SunflowerTargetPanelHideOnCapstoneOpenPatch
{
    public static void Postfix()
    {
        SunflowerManager.PushTargetButtonsHidden();
    }
}

[HarmonyPatch(typeof(NCapstoneSubmenuStack), nameof(NCapstoneSubmenuStack.AfterCapstoneClosed))]
public static class SunflowerTargetPanelShowOnCapstoneClosedPatch
{
    public static void Postfix()
    {
        SunflowerManager.PopTargetButtonsHidden();
    }
}

[HarmonyPatch(typeof(NCardRewardSelectionScreen), nameof(NCardRewardSelectionScreen.AfterOverlayOpened))]
public static class SunflowerTargetPanelHideOnCardRewardOpenPatch
{
    public static void Postfix()
    {
        SunflowerManager.PushTargetButtonsHidden();
    }
}

[HarmonyPatch(typeof(NCardRewardSelectionScreen), nameof(NCardRewardSelectionScreen.AfterOverlayClosed))]
public static class SunflowerTargetPanelShowOnCardRewardClosedPatch
{
    public static void Postfix()
    {
        SunflowerManager.PopTargetButtonsHidden();
    }
}

[HarmonyPatch(typeof(NChooseACardSelectionScreen), nameof(NChooseACardSelectionScreen.AfterOverlayOpened))]
public static class SunflowerTargetPanelHideOnChooseCardOpenPatch
{
    public static void Postfix()
    {
        SunflowerManager.PushTargetButtonsHidden();
    }
}

[HarmonyPatch(typeof(NChooseACardSelectionScreen), nameof(NChooseACardSelectionScreen.AfterOverlayClosed))]
public static class SunflowerTargetPanelShowOnChooseCardClosedPatch
{
    public static void Postfix()
    {
        SunflowerManager.PopTargetButtonsHidden();
    }
}

[HarmonyPatch(typeof(NCardGridSelectionScreen), nameof(NCardGridSelectionScreen.AfterOverlayOpened))]
public static class SunflowerTargetPanelHideOnCardGridOpenPatch
{
    public static void Postfix()
    {
        SunflowerManager.PushTargetButtonsHidden();
    }
}

[HarmonyPatch(typeof(NCardGridSelectionScreen), nameof(NCardGridSelectionScreen.AfterOverlayClosed))]
public static class SunflowerTargetPanelShowOnCardGridClosedPatch
{
    public static void Postfix()
    {
        SunflowerManager.PopTargetButtonsHidden();
    }
}
