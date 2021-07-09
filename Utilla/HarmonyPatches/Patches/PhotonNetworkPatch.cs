using HarmonyLib;
using Photon.Pun;

namespace Utilla.HarmonyPatches
{
	[HarmonyPatch(typeof(PhotonNetwork))]
    [HarmonyPatch("CreateRoom", MethodType.Normal)]
    internal class PhotonNetworkPatch
	{
        public static bool setCasualPrivate = false;

        private static void Prefix(ref Photon.Realtime.RoomOptions roomOptions)
		{
            if (setCasualPrivate)
			{
                if (roomOptions.CustomRoomProperties["gameMode"] as string == "private")
				{
                    roomOptions.CustomRoomProperties["gameMode"] = "privateCASUAL";
				}

                setCasualPrivate = false;
			}
		}
	}
}
