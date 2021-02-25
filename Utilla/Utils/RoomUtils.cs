using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace Utilla.Utils
{
    public static class RoomUtils
    {
        public static string RoomCode;

        internal static string RandomString(int length)
        {
            System.Random random = new System.Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void JoinPrivateLobby() => JoinPrivateLobby(RoomCode, PhotonNetworkController.instance);
        public static void JoinPrivateLobby(PhotonNetworkController __instance) => JoinPrivateLobby(RoomCode, __instance);
        public static void JoinPrivateLobby(string code) => JoinPrivateLobby(code, PhotonNetworkController.instance);
        public static void JoinPrivateLobby(string code, PhotonNetworkController __instance)
        {
            RoomCode = code;
            __instance.currentGameType = "privatetag";
            __instance.customRoomID = code;
            __instance.isPrivate = true;
            if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.Name != code)
            {
                PhotonNetworkController.instance.currentGorillaParent.GetComponentInChildren<GorillaScoreboardSpawner>().OnLeftRoom();
                __instance.attemptingToConnect = true;
                foreach (SkinnedMeshRenderer skinnedMeshRenderer2 in __instance.offlineVRRig)
                {
                    if (skinnedMeshRenderer2 != null)
                    {
                        skinnedMeshRenderer2.enabled = true;
                    }
                }
                PhotonNetwork.Disconnect();
            }
            if (!PhotonNetwork.InRoom && !__instance.attemptingToConnect)
            {
                __instance.attemptingToConnect = true;
                __instance.AttemptToConnectToRoom();
                UnityEngine.Debug.Log("attempting to connect");
            }
        }
    }
}
