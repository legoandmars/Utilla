// https://github.com/Not-A-Bird-07/Utilla/commit/c813503da35b39e63290a776af447e16a88d64c5

using GorillaGameModes;
using HarmonyLib;
using System;
using System.Linq;

namespace Utilla.Patches;

[HarmonyPatch(typeof(GorillaGameManager)), HarmonyWrapSafe]
internal class GameModePatches
{
    [HarmonyPatch(nameof(GorillaGameManager.GameTypeName)), HarmonyPrefix]
    public static bool GameTypeNamePatch(GorillaGameManager __instance, ref string __result)
    {
        if (Enum.IsDefined(typeof(GameModeType), (int)__instance.GameType())) return true;

        int index = GameMode.gameModeTable.LastOrDefault(pair => pair.Value == __instance).Key;
        __result = GameMode.gameModeKeyByName.LastOrDefault(pair => pair.Value == index).Key;

        return false;
    }
}