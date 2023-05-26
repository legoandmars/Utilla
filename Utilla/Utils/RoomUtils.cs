using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using System.Reflection;
using Utilla.HarmonyPatches;
using GorillaNetworking;
using BepInEx;

namespace Utilla.Utils
{
	public static class RoomUtils
	{
		public static string RoomCode;

		internal static string defaultQueue;

		static GorillaNetworkJoinTrigger joinTrigger;

		internal static string RandomString(int length)
		{
			System.Random random = new System.Random();
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			return new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
		}

		/// <summary>
		/// Joins a private room from a sepcifc room code.
		/// </summary>
		public static void JoinPrivateLobby() => JoinPrivateLobby(RoomCode, PhotonNetworkController.Instance);

		/// <inheritdoc cref="JoinPrivateLobby()"/>
		/// <param name="__instance">Instance of PhotonNetworkController to use.</param>
		public static void JoinPrivateLobby(PhotonNetworkController __instance) => JoinPrivateLobby(RoomCode, __instance);

		/// <inheritdoc cref="JoinPrivateLobby()"/>
		/// <param name="code">Room code to use.</param>
		public static void JoinPrivateLobby(string code) => JoinPrivateLobby(code, false);

		/// <inheritdoc cref="JoinPrivateLobby(string)"/>
		/// <param name="casual">Whether or not to make the room casual.</param>
		public static void JoinPrivateLobby(string code, bool casual = false) => JoinPrivateLobby(code, PhotonNetworkController.Instance, casual);

		/// <inheritdoc cref="JoinPrivateLobby(string, bool)"/>
		/// <inheritdoc cref="JoinPrivateLobby(PhotonNetworkController)"/>
		public static void JoinPrivateLobby(string code, PhotonNetworkController __instance, bool casual = false)
		{
			RoomCode = code;
			__instance.customRoomID = code;
			__instance.isPrivate = true;
			Debug.Log("attempting to connect");
			__instance.AttemptToJoinSpecificRoom(code);

			if (casual)
			{
				PhotonNetworkPatch.setCasualPrivate = true;
			}
			return;
		}

		/// <summary>
		/// Joins pseudo-public room using a queue.
		/// </summary>
		/// <param name="map">Name of the queue to use.</param>
		public static void JoinModdedLobby(string map) => JoinModdedLobby(map, false);
		
		/// <inheritdoc cref="JoinModdedLobby(string)"/>
		/// <param name="casual">Whether or not to make the room casual.</param>
		public static void JoinModdedLobby(string map, bool casual = false)
		{
			string gameModeName = "infection_MOD_" + map;
			PhotonNetworkController photonNetworkController = PhotonNetworkController.Instance;

			string queue = casual ? "CASUAL" : "DEFAULT";

			defaultQueue = GorillaComputer.instance.currentQueue;
			GorillaComputer.instance.currentQueue = queue;

			// Setting player prefs is not needed
			// PlayerPrefs.SetString("currentQueue", queue);
			// PlayerPrefs.Save();

			// What does this do?
			FieldInfo field = photonNetworkController.GetType().GetField("pastFirstConnection", BindingFlags.Instance | BindingFlags.NonPublic);
			field.SetValue(photonNetworkController, true);

			// Go to code_MAP for maps while in a private
			if (PhotonNetwork.InRoom && (PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] as string).Contains("private"))
			{
				string customRoomID = photonNetworkController.customRoomID;
				if (!customRoomID.Contains("_MAP"))
				{
					Debug.Log("JOINING");
					JoinPrivateLobby(customRoomID + "_MAP", casual);
					return;
				}
			}

			//photonNetworkController.currentGameType = gameModeName;
			if (joinTrigger == null)
			{
				joinTrigger = new GameObject("UtillaJoinTrigger").AddComponent<GorillaNetworkJoinTrigger>();
				joinTrigger.makeSureThisIsDisabled = Array.Empty<GameObject>();
				joinTrigger.makeSureThisIsEnabled = Array.Empty<GameObject>();
				joinTrigger.joinScreens = Array.Empty<GorillaLevelScreen>();
				joinTrigger.leaveScreens = Array.Empty<GorillaLevelScreen>();
			}
			joinTrigger.gameModeName = gameModeName;
			photonNetworkController.AttemptToJoinPublicRoom(joinTrigger);
		}

		internal static void ResetQueue()
		{
            if (!defaultQueue.IsNullOrWhiteSpace())
			{
                GorillaComputer.instance.currentQueue = RoomUtils.defaultQueue;
				defaultQueue = null;
			}
		}
	}
}
