﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class GameRoom
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = chat + $"{chat} I am {packet.playerId}";
            ArraySegment<byte> segment = packet.Write();

            lock (_lock)//공유변수 시작하므로 락.
            {
                foreach (ClientSession s in _sessions)
                    s.Send(segment);
            }
        }

        public void Enter(ClientSession session)
        {
            lock(_lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
            }
        }

    }
}
