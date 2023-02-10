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

            PlayerInfoReq packet = new PlayerInfoReq() { size = 4, packetId = (ushort)PacketID.PlayerInfoReq };


            // 보낸다(손님이 먼저 보냄)
            //for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> s = SendBufferHelper.Open(4096); // 한번에 큰 덩어리. new byte[4096];  
                
                byte[] size = BitConverter.GetBytes(packet.size); //2
                byte[] packetId = BitConverter.GetBytes(packet.packetId); //2 
                byte[] playerId = BitConverter.GetBytes(packet.playerId); //8

                ushort count = 0;
                Array.Copy(size, 0, s.Array, s.Offset+ count, 2);
                count += 2;
                Array.Copy(packetId, 0, s.Array, s.Offset + count, 2);
                count += 2;
                Array.Copy(playerId, 0, s.Array, s.Offset + count, 8);
                count += 8;
                ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);

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
