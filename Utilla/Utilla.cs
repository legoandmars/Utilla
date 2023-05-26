using System;
using BepInEx;
using Utilla.HarmonyPatches;
using Utilla.Utils;
using UnityEngine;
using System.Linq;
using Photon.Realtime;
using Photon.Pun;

namespace Utilla
{

	[BepInPlugin("org.legoandmars.gorillatag.utilla", "Utilla", "1.6.8")]
    public class Utilla : BaseUnityPlugin
    {
        static Events events = new Events();

        void Start()
        {
            RoomUtils.RoomCode = RoomUtils.RandomString(6); // Generate a random room code in case we need it

            GameObject dataObject = new GameObject();
            DontDestroyOnLoad(dataObject);
            gameObject.AddComponent<UtillaNetworkController>();

            Events.GameInitialized += PostInitialized;

            UtillaNetworkController.events = events;
            PostInitializedPatch.events = events;

            UtillaPatches.ApplyHarmonyPatches();
        }

        void PostInitialized(object sender, EventArgs e)
		{
			new GameObject("CustomGamemodesManager").AddComponent<GamemodeManager>();
		}
    }
}
