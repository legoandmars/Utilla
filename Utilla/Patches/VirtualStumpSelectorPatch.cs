using GorillaTagScripts.VirtualStumpCustomMaps;
using HarmonyLib;
using Utilla.Behaviours;

namespace Utilla.Patches
{
    [HarmonyPatch(typeof(CustomMapModeSelector)), HarmonyWrapSafe]
    internal class VirtualStumpSelectorPatch
    {
        [HarmonyPatch(nameof(CustomMapModeSelector.OnEnable)), HarmonyPrefix]
        public static void OnEnablePrefix()
        {
            GorillaComputerPatches.AllowSettingMode = false;
        }

        [HarmonyPatch(nameof(CustomMapModeSelector.OnEnable)), HarmonyPostfix]
        public static void OnEnablePostfix(CustomMapModeSelector __instance)
        {
            GorillaComputerPatches.AllowSettingMode = true;

            if (__instance.TryGetComponent(out UtillaGamemodeSelector selector)) selector.CheckGameMode();
            else __instance.AddComponent<UtillaGamemodeSelector>();
        }

        [HarmonyPatch(nameof(CustomMapModeSelector.SetupButtons)), HarmonyPostfix]
        public static void SetupButtonsPatch(CustomMapModeSelector __instance)
        {
            if (!__instance.TryGetComponent(out UtillaGamemodeSelector selector)) return;
            selector.OnSelectorSetup();
        }

        [HarmonyPatch(nameof(CustomMapModeSelector.ResetButtons)), HarmonyPrefix]
        public static void ResetButtonsPrefix()
        {
            GorillaComputerPatches.AllowSettingMode = false;
        }

        [HarmonyPatch(nameof(CustomMapModeSelector.ResetButtons)), HarmonyPostfix]
        public static void ResetButtonsPostfix()
        {
            GorillaComputerPatches.AllowSettingMode = true;

            foreach (CustomMapModeSelector instance in CustomMapModeSelector.instances)
            {
                if (!instance.TryGetComponent(out UtillaGamemodeSelector selector)) continue;
                selector.CheckGameMode();
            }
        }

        [HarmonyPatch(nameof(CustomMapModeSelector.SetAvailableGameModes)), HarmonyPrefix]
        public static void AvailableModesPrefix()
        {
            GorillaComputerPatches.AllowSettingMode = false;
        }

        [HarmonyPatch(nameof(CustomMapModeSelector.SetAvailableGameModes)), HarmonyPostfix]
        public static void AvailableModesPostfix()
        {
            GorillaComputerPatches.AllowSettingMode = true;

            foreach (CustomMapModeSelector instance in CustomMapModeSelector.instances)
            {
                if (!instance.TryGetComponent(out UtillaGamemodeSelector selector)) continue;
                selector.CheckGameMode();
            }
        }
    }
}
