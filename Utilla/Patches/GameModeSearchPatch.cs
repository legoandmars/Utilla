using GorillaGameModes;
using HarmonyLib;
using Utilla.Models;
using Utilla.Tools;
using Utilla.Utils;

namespace Utilla.Patches;

[HarmonyPatch(typeof(GameMode), nameof(GameMode.FindGameModeInString))]
internal class GameModeSearchPatch
{
    public static bool Prefix(string gmString, ref string __result)
    {
        if (GameModeUtils.FindGamemodeInString(gmString) is Gamemode gamemode)
        {
            __result = gamemode.ID;
            return false;
        }

        Logging.Error("NOT GOOD");
        return true;
    }
}
