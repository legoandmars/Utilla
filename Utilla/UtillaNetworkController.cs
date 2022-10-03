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
			public string assemblyHash;

        }

        public static Events events;

        Events.RoomJoinedArgs lastRoom;

		private string assemblyHash;
		private string AssemblyHash {
			get {
				if (assemblyHash != null) {
					return assemblyHash;
				} else {
					assemblyHash = GetAssemblyHash();
					return assemblyHash;
				}
			}
		}
  
		public override void OnJoinedRoom()
		{
            // trigger events
            bool isPrivate = false;
            string gamemode = "";
            if (PhotonNetwork.CurrentRoom != null)
            {
                var currentRoom = PhotonNetwork.NetworkingClient.CurrentRoom;
                isPrivate = !currentRoom.IsVisible ||
                            currentRoom.CustomProperties.ContainsKey("Description"); // Room Browser rooms
				if (currentRoom.CustomProperties.TryGetValue("gameMode", out var gamemodeObject))
				{
                    gamemode = gamemodeObject as string;
				}
            }

			// TODO: Generate dynamically
			var prefix = "ERROR";
			if (gamemode.Contains(Models.Gamemode.GamemodePrefix))
			{
				prefix = "CUSTOM";
            }
			else
            {
                var dict = new Dictionary<string, string> {
					{ "INFECTION", "INFECTION" },
                    { "CASUAL", "CASUAL"},
                    { "HUNT", "HUNT" },
                    { "BATTLE", "PAINTBRAWL"},
				};

				foreach (var item in dict)
                {
					if (gamemode.Contains(item.Key))
                    {
						prefix = item.Value;
						break;
                    }
                } 
            }
			GorillaComputer.instance.currentGameModeText.text = "CURRENT MODE\n" + prefix;

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
			mods.assemblyHash = AssemblyHash;
			table.Add("mods", JsonUtility.ToJson(mods));
			PhotonNetwork.LocalPlayer.SetCustomProperties(table);

			RoomUtils.ResetQueue();
        }

		private string GetAssemblyHash()
		{
			string hash = "";
			string hashPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

			hashPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(hashPath, @"../../../../Gorilla Tag_Data/Managed/Assembly-CSharp.dll"));

			byte[] assemblyBytes = System.IO.File.ReadAllBytes(hashPath);

			System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create();

			byte[] ShaByte = sha.ComputeHash(assemblyBytes);
			hash = System.Convert.ToBase64String(ShaByte);

            return hash;
		}

		public override void OnLeftRoom()
		{
            if (lastRoom != null)
			{
				events.TriggerRoomLeft(lastRoom);
				lastRoom = null;
			}

			GorillaComputer.instance.currentGameModeText.text = "CURRENT MODE\n-NOT IN ROOM-";
		}
	}
}
