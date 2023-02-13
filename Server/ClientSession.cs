using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            //Packet packet = new Packet() { size = 100, packetId = 10 };

            //// [ ][ ][ ][ ][ ][ ][ ][ ]
            //// [    100   ][    10    ]
            //// hp, attack에 대해서 각각 만들고 있는 부분은 나중에 serialize해서? 자동으로 하도록 바꾸게 될 것임..
            //// hp, attack 대신에 size, packetId로 바꿨음.
            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096); // 한번에 큰 덩어리. new byte[4096];  
            //byte[] buffer = BitConverter.GetBytes(packet.size);
            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
            //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);


            //// 이동패킷 100명
            //// Session외부에 두었기 때문에 1개 -> 1 * 100명
            //// Session내부에 두었다면 100개 세션 -> 100 * 100명 (= 1만개). 비효율.
            //Send(sendBuff);
            Thread.Sleep(1000);
            Disconnect();
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);//패킷매니저 싱글톤을 호출.
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");

        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
