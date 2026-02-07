using System;

namespace Utilla
{
    public class Events
    {
        public static Events Instance = new();

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
            RoomJoined?.SafeInvoke(this, e);
        }

        public virtual void TriggerRoomLeft(RoomJoinedArgs e)
        {
            RoomLeft?.SafeInvoke(this, e);
        }

        public virtual void TriggerGameInitialized()
        {
            GameInitialized?.SafeInvoke(this, EventArgs.Empty);
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