using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class PacketHandler
{
    //어떤 세션에서 조립된 어떤 패킷인지를 인자로 받음.
    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
            return;

        //jobqueue에 쌓이는 주문서.

        GameRoom room = clientSession.Room;
        room.Push(
            () => room.Broadcast(clientSession, chatPacket.chat)
        );
    }

}

