using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Utilla.Utils;

namespace Utilla.Behaviours
{
    internal class UtillaNetworkController : MonoBehaviourPunCallbacks
    {
        public static UtillaNetworkController Instance { get; private set; }

        private Events.RoomJoinedArgs lastRoom;

        public override void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            base.OnEnable(); // Tell Photon to register this object as a callback target, this will be important shortly

            if (NetworkSystem.Instance is NetworkSystem netSys && netSys is NetworkSystemPUN && PhotonNetwork.NetworkingClient is LoadBalancingClient client)
            {
                // The following code inserts our callbacks right before the network system does
                // This ensures any relative members a part of Utilla are properly defined before anything else gets their values

                client.UpdateCallbackTargets();
                MatchMakingCallbacksContainer callbackContainer = client.MatchMakingCallbackTargets;

                for (int i = 0; i < callbackContainer.Count; i++)
                {
                    IMatchmakingCallbacks individualCallback = callbackContainer[i];
                    if ((object)individualCallback is MonoBehaviour behaviour && behaviour.gameObject == netSys.gameObject)
                    {
                        if (callbackContainer.Contains(this)) callbackContainer.Remove(this);
                        callbackContainer.Insert(i, this);
                        break;
                    }
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (Instance == this) Instance = null;
        }

        public override void OnJoinedRoom()
        {
            if (ApplicationQuittingState.IsQuitting) return;

            // trigger events

            NetworkSystem netSys = NetworkSystem.Instance;
            bool isPrivate = netSys.SessionIsPrivate;
            string gameMode = netSys.GameModeString;

            GameModeUtils.CurrentGamemode = GameModeUtils.FindGamemodeInString(gameMode);

            Events.RoomJoinedArgs args = new()
            {
                isPrivate = isPrivate,
                Gamemode = gameMode
            };
            Events.Instance.TriggerRoomJoin(args);

            lastRoom = args;

            //RoomUtils.ResetQueue();
        }

        public override void OnLeftRoom()
        {
            if (ApplicationQuittingState.IsQuitting) return;

            GameModeUtils.CurrentGamemode = null;

            if (lastRoom != null)
            {
                Events.Instance.TriggerRoomLeft(lastRoom);
                lastRoom = null;
            }
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            if (ApplicationQuittingState.IsQuitting || !NetworkSystem.Instance.InRoom || NetworkSystem.Instance.GameModeString is not string gameMode || gameMode == null) return;

            GameModeUtils.CurrentGamemode = GameModeUtils.FindGamemodeInString(gameMode);

            if (lastRoom.Gamemode != gameMode || lastRoom.isPrivate != NetworkSystem.Instance.SessionIsPrivate)
            {
                GamemodeManager.Instance.OnRoomLeft(null, lastRoom);

                lastRoom.Gamemode = gameMode;
                lastRoom.isPrivate = NetworkSystem.Instance.SessionIsPrivate;

                GamemodeManager.Instance.OnRoomJoin(null, lastRoom);
            }
        }
    }
}
