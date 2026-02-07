// https://github.com/developer9998/Utilla/pull/9/changes/8508d580682da5db1d1dae91b13e4591962832d5

using GorillaGameModes;
using HarmonyLib;
using System;
using System.Reflection;
using Utilla.Utils;

namespace Utilla.Patches;

[HarmonyPatch]
public class EnumNamePatch
{
    public static MethodBase TargetMethod()
    {
        return typeof(EnumUtilExt)
            .GetMethod(nameof(EnumUtilExt.GetName), BindingFlags.Public | BindingFlags.Static)
            ?.MakeGenericMethod(typeof(GameModeType));
    }

    public static bool Prefix(GameModeType e, ref string __result)
    {
        if (!Enum.IsDefined(typeof(GameModeType), (int)e))
        {
            __result = GameModeUtils.GetGameModeInstance(e).GameTypeName();
            return false;
        }

        return true;
    }
}
