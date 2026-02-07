using GorillaGameModes;
using GorillaNetworking;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Utilla.Models;
using Utilla.Tools;
using Utilla.Utils;

namespace Utilla.Patches
{
    [HarmonyPatch(typeof(GorillaComputer))]
    internal static class GorillaComputerPatches
    {
        public static bool AllowSettingMode { get; internal set; }

        public static string ModePreferenceKey { get; private set; }

        [HarmonyPatch(nameof(GorillaComputer.SetGameModeWithoutButton)), HarmonyPrefix, HarmonyPriority(Priority.First)]
        public static bool SetModePatch(string gameMode, MethodBase __originalMethod)
        {
            StackTrace trace = new();
            StackFrame frame = trace.GetFrame(2);
            if (frame != null && frame.GetMethod() is MethodBase methodBase && methodBase.DeclaringType.FullName.Contains("GameModeSelectorButtonLayout")) return false;

            return AllowSettingMode;
        }

        [HarmonyPatch(nameof(GorillaComputer.UpdateGameModeText)), HarmonyPrefix]
        public static bool UpdateModeTextPatch(GorillaComputer __instance)
        {
            WatchableStringSO currentGameModeText = __instance.currentGameModeText;

            LocalisationManager.TryGetKeyForCurrentLocale("CURRENT_MODE", out string currentMode, "CURRENT MODE");

            if (!(NetworkSystem.Instance?.InRoom ?? false))
            {
                LocalisationManager.TryGetKeyForCurrentLocale("NOT_IN_ROOM", out string notInRoom, "-NOT IN ROOM-");
                currentGameModeText.Value = $"{currentMode}\n{notInRoom}";
                return false;
            }

            Gamemode gamemode = GameModeUtils.CurrentGamemode;
            currentGameModeText.Value = $"{currentMode}\n{(gamemode is not null ? gamemode.DisplayName.ToUpper() : GorillaScoreBoard.error)}";

            return false;
        }

        [HarmonyPatch(nameof(GorillaComputer.InitializeGameMode), argumentTypes: []), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> InitializeModeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            if (ModePreferenceKey != null && ModePreferenceKey.Length > 0) return instructions;

            CodeInstruction[] codes = [.. instructions];

            for (int i = 0; i < codes.Length; i++)
            {
                if (codes[i].opcode == OpCodes.Call)
                {
                    object operand = codes[i].operand;
                    Type opType = operand.GetType();

                    string methodName = (string)AccessTools.Property(opType, "Name").GetValue(operand);
                    Type returnType = (Type)AccessTools.Property(opType, "ReturnType").GetValue(operand);
                    Type declaringType = (Type)AccessTools.Property(opType, "DeclaringType").GetValue(operand);

                    if (methodName != "GetString" || returnType != typeof(string) || declaringType != typeof(PlayerPrefs)) continue;

                    CodeInstruction code = codes.Take(i + 1).LastOrDefault(code => code.opcode == OpCodes.Ldstr);
                    if (code != null)
                    {
                        ModePreferenceKey = (string)code.operand;
                        break;
                    }
                }
            }

            return codes;
        }

        [HarmonyPatch(nameof(GorillaComputer.InitializeGameMode), argumentTypes: []), HarmonyPrefix]
        internal static bool InitializeModePatch(GorillaComputer __instance)
        {
            if (!__instance.didInitializeGameMode)
            {
                string gameMode = PlayerPrefs.GetString(ModePreferenceKey, GameModeType.Infection.ToString());
                Logging.Message($"Initial Game Mode: {gameMode}");

                GorillaComputer.sessionCount = 100;
                __instance.leftHanded = PlayerPrefs.GetInt("leftHanded", 0) == 1;
                __instance.OnModeSelectButtonPress(gameMode, __instance.leftHanded);
                GameModePages.SetSelectedGameModeShared(gameMode);

                __instance.didInitializeGameMode = true;
            }

            return false;
        }
    }
}
