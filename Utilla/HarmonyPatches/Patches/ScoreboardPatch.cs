using HarmonyLib;
using UnityEngine;

namespace Utilla.HarmonyPatches
{
	[HarmonyPatch(typeof(GorillaScoreboardSpawner))]
	[HarmonyPatch("OnJoinedRoom")] // Just a guess
	internal class ScoreboardPatch
	{
		private static void Prefix(GorillaScoreboardSpawner __instance)
		{
			if (__instance.notInRoomText == null)
			{
				__instance.notInRoomText = new GameObject();
			}
		}
	}
}
