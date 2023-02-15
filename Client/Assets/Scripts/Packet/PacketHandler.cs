using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class PacketHandler
{
    //어떤 세션에서 조립된 어떤 패킷인지를 인자로 받음.
    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        UnityEngine.Debug.Log(chatPacket.chat);
        ////if (chatPacket.playerId == 1)
        //    Console.WriteLine(chatPacket.chat);
    }
}


