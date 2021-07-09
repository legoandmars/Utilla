using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Utilla.HarmonyPatches
{
	[HarmonyPatch(typeof(GorillaLevelScreen))]
	[HarmonyPatch("UpdateText")] // Just a guess
	internal class GorillaLevelScreenPatch
	{
		private static bool Prefix(GorillaLevelScreen __instance, ref Text ___myText)
		{
			if (___myText == null || __instance.GetComponent<MeshRenderer>() == null)
			{
				return false;
			}
			return true;
		}
	}
}
