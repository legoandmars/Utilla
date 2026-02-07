using GorillaGameModes;
using HarmonyLib;
using System;
using System.Reflection;
using Utilla.Utils;
using Utilla.Models;

namespace Utilla.Patches;

[HarmonyPatch]
internal class EnumParsePatch
{
    public static MethodBase TargetMethod()
    {
        return typeof(Enum)
            .GetMethod(nameof(Enum.Parse), BindingFlags.Public | BindingFlags.Static, null, [typeof(string), typeof(bool)], null)
            ?.MakeGenericMethod(typeof(GameModeType));
    }

    public static bool Prefix(string value, ref object __result)
    {
        if (GameModeUtils.GetGamemodeFromId(value) is Gamemode gamemode)
        {
            __result = gamemode.BaseGamemode.GetValueOrDefault(GameModeType.Infection);
            return false;
        }

        EnumData<GameModeType> shared = EnumData<GameModeType>.Shared;
        __result = shared.NameToEnum.TryGetValue(value, out var gameMode) ? gameMode : GameModeType.Infection;

        return false;
    }
}
