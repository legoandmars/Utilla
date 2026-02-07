using GorillaGameModes;
using GorillaNetworking;
using HarmonyLib;
using System;
using Utilla.Models;
using Utilla.Tools;
using Utilla.Utils;

namespace Utilla.Patches
{
    [HarmonyPatch(typeof(GorillaNetworkJoinTrigger))]
    internal class DesiredGameModePatches
    {
        [HarmonyPatch(nameof(GorillaNetworkJoinTrigger.GetDesiredGameType)), HarmonyPrefix]
        public static bool DesiredGameTypePatch(GorillaNetworkJoinTrigger __instance, ref string __result, ref GTZone ___zone)
        {
            Type joinTriggerType = __instance.GetType();

            Logging.Info($"{joinTriggerType.Name}.{nameof(GorillaNetworkJoinTrigger.GetDesiredGameType)}");

            if (joinTriggerType == typeof(GorillaNetworkRankedJoinTrigger) || ___zone == GTZone.ranked)
            {
                Logging.Message($"Ranked JoinTrigger resorting to hardcoded infection mode");
                __result = GameModeType.InfectionCompetitive.ToString();
                return false;
            }

            string currentGameMode = GorillaComputer.instance.currentGameMode.Value;

            if (!Enum.IsDefined(typeof(GameModeType), currentGameMode))
            {
                if (GameModeUtils.GetGamemodeFromId(currentGameMode) is Gamemode gamemode && gamemode.BaseGamemode.HasValue && gamemode.BaseGamemode.Value < GameModeType.Count)
                {
                    GameModeType gameModeType = gamemode.BaseGamemode.Value;

                    GameModeType verifiedGameMode = GameMode.GameModeZoneMapping.VerifyModeForZone(__instance.zone, gameModeType, NetworkSystem.Instance.SessionIsPrivate);
                    if (verifiedGameMode == gameModeType)
                    {
                        Logging.Message($"JoinTrigger of {___zone.GetName()} allowing generic game mode: {currentGameMode} under {gameModeType}");
                        __result = currentGameMode;
                        return false;
                    }

                    Logging.Message($"JoinTrigger of {___zone.GetName()} changing unsupported game mode: {currentGameMode} under {gameModeType}");
                    __result = verifiedGameMode.ToString();
                    return false;
                }

                Logging.Message($"JoinTrigger of {___zone.GetName()} allowing custom game mode: {currentGameMode}");
                __result = currentGameMode;
                return false;
            }

            Logging.Message($"JoinTrigger of {___zone.GetName()} naturally allows game mode: {currentGameMode}");
            return true;
        }

        [HarmonyPatch(nameof(GorillaNetworkJoinTrigger.GetDesiredGameTypeLocalized)), HarmonyPrefix]
        public static bool DesiredLocalizedGameTypePatch(GorillaNetworkJoinTrigger __instance, ref string __result, ref GTZone ___zone) => DesiredGameTypePatch(__instance, ref __result, ref ___zone);
    }
}
