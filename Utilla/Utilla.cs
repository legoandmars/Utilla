using System;
using BepInEx;
using Utilla.HarmonyPatches;
using Utilla.Utils;
using UnityEngine;

namespace Utilla
{
	[BepInPlugin("org.legoandmars.gorillatag.utilla", "Utilla", "1.6.14")]
    public class Utilla : BaseUnityPlugin
    {
        static Events events = new Events();

        private UtillaNetworkController _networkController;

        void Start()
        {
            UtillaLogging.Logger = Logger;

            DontDestroyOnLoad(this);
            RoomUtils.RoomCode = RoomUtils.RandomString(6); // Generate a random room code in case we need it

            _networkController = gameObject.AddComponent<UtillaNetworkController>();

            Events.GameInitialized += PostInitialized;

            UtillaNetworkController.events = events;
            PostInitializedPatch.events = events;

            UtillaPatches.ApplyHarmonyPatches();
        }

        void PostInitialized(object sender, EventArgs e)
		{
            var go = new GameObject("CustomGamemodesManager");
            GameObject.DontDestroyOnLoad(go);

            var gmm = go.AddComponent<GamemodeManager>();

            _networkController.gameModeManager = gmm;
		}
    }
}
