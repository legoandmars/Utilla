using HarmonyLib;
using Utilla.Models;
using Utilla.Utils;

namespace Utilla.Patches
{
    [HarmonyPatch(typeof(GorillaScoreBoard), nameof(GorillaScoreBoard.RoomType)), HarmonyPriority(Priority.VeryHigh)]
    internal class ScoreBoardRoomNamePatch
    {
        public static bool Prefix(ref string __result)
        {
            Gamemode gamemode = GameModeUtils.CurrentGamemode;

            if (gamemode != null)
            {
                __result = gamemode.DisplayName.ToUpper();
                return false;
            }

            return true;
        }
    }
}
