using HarmonyLib;
using System.Linq;

namespace Utilla.HarmonyPatches
{
	[HarmonyPatch(typeof(NetworkSystemPUN), "get_lowestPingRegionIndex", MethodType.Normal)]
	internal class PlayerRegionPatch
	{
		public static bool Prefix(ref int __result, ref NetworkRegionInfo[] ___regionData)
		{
			__result = ___regionData.Select(data => data.playersInRegion).Max();
            return false;
        }
	}
}
