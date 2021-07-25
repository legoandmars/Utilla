using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using BepInEx;
using System.Reflection;
using Photon.Pun;
using UnityEngine;
using System.Linq;
using Utilla.Utils;

namespace Utilla.HarmonyPatches
{
	[HarmonyPatch(typeof(PhotonNetworkController))]
    [HarmonyPatch("OnJoinedRoom", MethodType.Normal)]
    internal class PhotonNetworkControllerPatch
    {
        public static Events events;
        private static void Postfix()
        {
            // trigger events
            bool isPrivate = false;
            if(PhotonNetwork.CurrentRoom != null)
            {
                var currentRoom = PhotonNetwork.NetworkingClient.CurrentRoom;
                isPrivate = !currentRoom.IsVisible ||
                            currentRoom.CustomProperties.ContainsKey("Description"); // Room Browser rooms
            }
            Debug.Log("IS PRIVATE?");
            Debug.Log(isPrivate);
            Events.RoomJoinedArgs args = new Events.RoomJoinedArgs();
            args.isPrivate = isPrivate;
            events.TriggerRoomJoin(args);

            // handle forcing private lobbies
            bool forcePrivateLobbies = false;
            var infos = BepInEx.Bootstrap.Chainloader.PluginInfos;
            foreach (var info in infos)
            {
                if (info.Value == null) continue;
                BaseUnityPlugin plugin = info.Value.Instance;
                if (plugin == null) continue;
                var attribute = plugin.GetType().GetCustomAttribute<ForcePrivateLobbyAttribute>();
                if (attribute != null) forcePrivateLobbies = true;
            }

            if (forcePrivateLobbies)
            {
                RoomUtils.JoinPrivateLobby();
            }

            // Reset the queue to what it was before
            if (!RoomUtils.defaultQueue.IsNullOrWhiteSpace())
			{
                GorillaComputer.instance.currentQueue = RoomUtils.defaultQueue;
			}
        }
    }
}
