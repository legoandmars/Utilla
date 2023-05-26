using UnityEngine;
using HarmonyLib;
using System.Collections;
using System.Threading.Tasks;

namespace Utilla.HarmonyPatches
{
	[HarmonyPatch(typeof(GorillaTagger))]
    [HarmonyPatch("Awake", MethodType.Normal)]
    static class PostInitializedPatch
	{
		public static Events events;

		private async static void Postfix()
        {
			await Task.Yield();
            events = new Events();
            events.TriggerGameInitialized();
        }
    }
}
