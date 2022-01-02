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
using GorillaNetworking;
using ExitGames.Client.Photon;

namespace Utilla
{
    public class UtillaNetworkController : MonoBehaviourPunCallbacks
    {
		[Serializable]
		public class DataClass
		{
			public string[] installedIDs;
			public string[] known;
		}

        public static Events events;

        Events.RoomJoinedArgs lastRoom;

		public override void OnJoinedRoom()
		{
            // trigger events
            bool isPrivate = false;
            string gamemode = "";
            if(PhotonNetwork.CurrentRoom != null)
            {
                var currentRoom = PhotonNetwork.NetworkingClient.CurrentRoom;
                isPrivate = !currentRoom.IsVisible ||
                            currentRoom.CustomProperties.ContainsKey("Description"); // Room Browser rooms
				if (currentRoom.CustomProperties.TryGetValue("gameMode", out var gamemodeObject))
				{
                    gamemode = gamemodeObject as string;
				}
            }

			Events.RoomJoinedArgs args = new Events.RoomJoinedArgs
            {
                isPrivate = isPrivate,
                Gamemode = gamemode
            };
            events.TriggerRoomJoin(args);

            lastRoom = args;

			var table = new Hashtable();
			var mods = new DataClass();
			mods.installedIDs = BepInEx.Bootstrap.Chainloader.PluginInfos.Select(x => x.Value.Metadata.GUID).ToArray();
			table.Add("mods", JsonUtility.ToJson(mods));
			PhotonNetwork.LocalPlayer.SetCustomProperties(table);

            // Reset the queue to what it was before
            if (!RoomUtils.defaultQueue.IsNullOrWhiteSpace())
			{
                GorillaComputer.instance.currentQueue = RoomUtils.defaultQueue;
			}
        }

		public override void OnLeftRoom()
		{
            if (lastRoom != null)
			{
				events.TriggerRoomLeft(lastRoom);
				lastRoom = null;
			}
		}
	}
}
