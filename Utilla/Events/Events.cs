using System;
using System.Collections.Generic;
using System.Text;

namespace Utilla
{
    public class Events
    {
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
            public bool isPrivate { get; set; }
        }

    }
}