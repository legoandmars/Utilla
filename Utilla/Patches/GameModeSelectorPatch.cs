using HarmonyLib;
using Utilla.Behaviours;

namespace Utilla.Patches
{
    [HarmonyPatch(typeof(GameModeSelectorButtonLayout))]
    internal class GameModeSelectorPatch
    {
        [HarmonyPatch(nameof(GameModeSelectorButtonLayout.OnEnable)), HarmonyPrefix]
        public static bool OnEnablePatch(GameModeSelectorButtonLayout __instance)
        {
            if (__instance.superToggleButton is GorillaPressableButton superToggleButton)
            {
                superToggleButton.onPressed += __instance._OnPressedSuperToggleButton;
            }

            __instance.SetupButtons();

            if (__instance.TryGetComponent(out UtillaGamemodeSelector selector))
            {
                selector.CheckGameMode();
                selector.ShowPage();
                return false;
            }

            __instance.AddComponent<UtillaGamemodeSelector>();
            return false;
        }

        [HarmonyPatch(nameof(GameModeSelectorButtonLayout.SetupButtons)), HarmonyPrefix]
        public static void SetupButtonsPrefix(GameModeSelectorButtonLayout __instance)
        {
            NetworkSystem.Instance.OnJoinedRoomEvent -= __instance.SetupButtons;
        }

        [HarmonyPatch(nameof(GameModeSelectorButtonLayout.SetupButtons)), HarmonyPostfix]
        public static void SetupButtonsPostfix(GameModeSelectorButtonLayout __instance)
        {
            if (!__instance.TryGetComponent(out UtillaGamemodeSelector selector)) return;
            selector.OnSelectorSetup();
        }
    }
}