using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using BepInEx;
using System.Reflection;
using System.Diagnostics;
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
        private static void Postfix(PhotonNetworkController __instance)
        {
            // trigger events
            bool isPrivate = false;
            if(PhotonNetwork.CurrentRoom != null)
            {
                isPrivate = !PhotonNetwork.CurrentRoom.IsVisible;
            }
            Events.TriggerRoomJoin(isPrivate);

            // handle forcing private lobbies
            bool forcePrivateLobbies = false;
            var infos = BepInEx.Bootstrap.Chainloader.PluginInfos;
            foreach(var info in infos)
            {
                if (info.Value == null) continue;
                BaseUnityPlugin plugin = info.Value.Instance;
                if (plugin == null) continue;
                var attribute = plugin.GetType().GetCustomAttribute<ForcePrivateLobbyAttribute>();
                if (attribute != null) forcePrivateLobbies = true;
            }

            if (forcePrivateLobbies)
            {
                RoomUtils.JoinPrivateLobby(__instance);
            }
        }
    }
}
