using System;
using System.Collections.Generic;
using System.Text;

namespace Utilla
{
    public static class Events
    {
        public static event Action<bool> RoomJoined;

        internal static void TriggerRoomJoin(bool isPrivate)
        {
            try
            {
                UnityEngine.Debug.Log("Joining a room!");
                UnityEngine.Debug.Log($"Private: {isPrivate}");
            }
            catch(Exception e)
            {
                isPrivate = false;
            }
            RoomJoined(isPrivate);
        }
    }
}