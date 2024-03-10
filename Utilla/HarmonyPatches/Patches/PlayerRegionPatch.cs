using HarmonyLib;
using System.Linq;

namespace Utilla.HarmonyPatches
{
	[HarmonyPatch(typeof(NetworkSystemPUN), "get_lowestPingRegionIndex", MethodType.Normal)]
	internal class PlayerRegionPatch
	{
		public static void Postfix(ref int __result)
		{
            NetworkRegionInfo[] regionInfo = (NetworkRegionInfo[])AccessTools.Field(typeof(NetworkSystemPUN), "regionData").GetValue((NetworkSystemPUN)NetworkSystem.Instance);
			int maxPlayersInRegions = regionInfo.Select(data => data.playersInRegion).Max();
			__result = regionInfo.IndexOfRef(regionInfo.First(data => data.playersInRegion == maxPlayersInRegions));
        }
	}
}
