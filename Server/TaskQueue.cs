using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    interface ITask
    {
        void Execute();
    }

    class BroadcastTask : ITask
    {
        GameRoom _room;
        ClientSession _session;
        string _chat;

        BroadcastTask (ClientSession session, string chat)
        {
            _room =_room;
            _session = session;
            _chat = chat;
        }
        public void Execute() 
        {
            _room.Broadcast(_session, _chat);
        }
    }
    class TaskQueue
    {
        Queue<ITask> _queue = new Queue<ITask>();
    }


}
