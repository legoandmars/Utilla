using System;
using BepInEx;
using Utilla.HarmonyPatches;
using System.Reflection;
using Utilla.Utils;
using UnityEngine;
using System.Linq;
using Photon.Realtime;
using Photon.Pun;

namespace Utilla
{

	[BepInPlugin("org.legoandmars.gorillatag.utilla", "Utilla", "1.4.0")]
    public class Utilla : BaseUnityPlugin
    {
        static Events events = new Events();

        void Awake()
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

    [ModdedGamemode]
    [ModdedGamemode("testqueue", "Test Queue", typeof(TestGameManager))]
	[BepInPlugin("test.com.graic", "Test MOD", "1.4.0")]
    public class TestMod : BaseUnityPlugin
    {
        void Awake()
        {
            Debug.Log("TestMod Loaded");
        }

        [ModdedGamemodeJoin]
        public void Join(string gamemode)
		{
            Debug.Log($"TestMod Queue Join {gamemode}");
            Debug.Log($"Materials: {string.Join(", ", GameObject.FindObjectOfType<VRRig>()?.materialsToChangeTo.Select(x => x.name))}");
		}

        [ModdedGamemodeLeave]
        public void Leave(string gamemode)
		{
            Debug.Log($"TestMod Queue Leave {gamemode}");
		}
    }

    public class TestGameManager : GorillaGameManager
	{
		public override string GameMode()
		{
            return "TEST";
		}

		public override int MyMatIndex(Player forPlayer)
		{
            return 3;
		}

		public override bool LocalCanTag(Player myPlayer, Player otherPlayer)
		{
            return false;
		}

        [PunRPC]
		public override void ReportTagRPC(Player taggedPlayer, PhotonMessageInfo info)
		{
            ReportTag(taggedPlayer, info.Sender);
		}

		public override void ReportTag(Player taggedPlayer, Player taggingPlayer)
		{
            if (!base.photonView.IsMine)
            {
                return;
            }
        }
	}
}
