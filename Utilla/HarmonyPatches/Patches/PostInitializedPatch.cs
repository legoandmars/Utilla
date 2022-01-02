using UnityEngine;
using HarmonyLib;
using System.Collections;

namespace Utilla.HarmonyPatches
{
	[HarmonyPatch(typeof(GorillaLocomotion.Player))]
    [HarmonyPatch("Awake", MethodType.Normal)]
    static class PostInitializedPatch
	{
		public static Events events;

		private static void Postfix(GorillaLocomotion.Player __instance)
        {
			__instance.StartCoroutine(DelayCoroutine());
		}

		static IEnumerator DelayCoroutine()
		{
			yield return 0;
			events.TriggerGameInitialized();
		}
    }
}
