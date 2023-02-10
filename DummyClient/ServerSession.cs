using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    class Packet
    {
        public ushort size; //ushort는 0 ~ 65,535, 2byte = 64KByte 까지.
        public ushort packetId;
    }

    class PlayerInfoReq : Packet
    {
        public long playerId;
    }

    class PlayerInfoOk : Packet
    {
        public int hp;
        public int attack;
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { packetId = (ushort)PacketID.PlayerInfoReq };//보내려는 최종 size는 미리 알수 없으므로 지웠음.

            // 보낸다(손님이 먼저 보냄)
            //for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> s = SendBufferHelper.Open(4096); // 한번에 큰 덩어리. new byte[4096];  

                ushort count = 0;
                bool success = true;

                //success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), packet.size);
                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset+count, s.Count-count), packet.packetId);
                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset+count, s.Count-count), packet.playerId);
                count += 8;
                success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), count); // 보내려는 최종 사이즈는 누적된 count.

                ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);

                if (success)
                    Send(sendBuff);

            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        // 이동 패킷 ((3,2) 좌표로 이동하고 싶다!)
        // 문자열 말고 정해진 규약. 프로토콜 대로 보내도록 바꾸자.
        // 패킷번호, 좌표: 15 3 2
        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
