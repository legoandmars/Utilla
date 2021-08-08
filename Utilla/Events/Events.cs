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

        public virtual void TriggerRoomJoin(RoomJoinedArgs e)
        {
            EventHandler<RoomJoinedArgs> handler = RoomJoined;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public class RoomJoinedArgs : EventArgs
        {
            /// <summary>
            /// Whether or not the room is private.
            /// </summary>
            public bool isPrivate { get; set; }
        }

    }
}