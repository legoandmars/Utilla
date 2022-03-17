using System;
using System.Collections.Generic;
using System.Text;

namespace Utilla
{
    public class Events
    {
        /// <summary>
        /// An event that gets called whenever a room is joined.
        /// </summary>
        public static event EventHandler<RoomJoinedArgs> RoomJoined;

        /// <summary>
        /// An event that gets called whenever a room is left.
        /// </summary>
        public static event EventHandler<RoomJoinedArgs> RoomLeft;

        /// <summary>
        /// An event that gets called whenever the game has finished initializing.
        /// </summary>
        public static event EventHandler GameInitialized;

        public virtual void TriggerRoomJoin(RoomJoinedArgs e)
        {
            try
			{
				RoomJoined?.Invoke(this, e);
			} catch (Exception ex)
			{
                UnityEngine.Debug.LogError($"Error in Utilla RoomJoined: {ex}");
			}
        }

        public virtual void TriggerRoomLeft(RoomJoinedArgs e)
		{
            try
			{
				RoomLeft?.Invoke(this, e);
			} catch (Exception ex)
			{
                UnityEngine.Debug.LogError($"Error in Utilla RoomLeft: {ex}");
			}
		}

        public virtual void TriggerGameInitialized()
		{
            try
			{
				GameInitialized?.Invoke(this, EventArgs.Empty);
			} catch (Exception ex)
			{
                UnityEngine.Debug.LogError($"Error in Utilla GameInitialized: {ex}");
			}
		}

        public class RoomJoinedArgs : EventArgs
        {
            /// <summary>
            /// Whether or not the room is private.
            /// </summary>
            public bool isPrivate { get; set; }

            /// <summary>
            /// The gamemode that the current lobby is 
            /// </summary>
            public string Gamemode { get; set; }
        }

    }
}