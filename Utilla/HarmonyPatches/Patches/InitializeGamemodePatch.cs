using HarmonyLib;
using GorillaNetworking;
using UnityEngine;

namespace Utilla.HarmonyPatches.Patches
{
    [HarmonyPatch(typeof(GorillaComputer))]
    [HarmonyPatch("InitializeGameMode")]
    internal class InitializeGamemodePatch
    {
        internal static bool Prefix(GorillaComputer __instance)
        {
            __instance.leftHanded = PlayerPrefs.GetInt("leftHanded", 0) == 1;
            __instance.OnModeSelectButtonPress(PlayerPrefs.GetString("currentGameMode", "INFECTION"), __instance.leftHanded);
            return false;
        }
    }
}
