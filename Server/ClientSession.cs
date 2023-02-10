﻿using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public abstract class Packet
    {
        public ushort size; //ushort는 0 ~ 65,535, 2byte = 64KByte 까지.
        public ushort packetId;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    class PlayerInfoReq : Packet
    {
        public long playerId;

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> s)
        {
            //역직렬화
            ushort count = 0;

            //ushort size = BitConverter.ToUInt16(s.Array, s.Offset);
            count += 2;
            //ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += 2;

            this.playerId = BitConverter.ToInt64(new ReadOnlySpan<byte>(s.Array, s.Offset + count, s.Count - count)); // count만 변조하는 악의적인 공격을 감지하기 위해서 누적count보다 header리턴하는 count가 작으면 에러가 나도록 수정함.
            count += 8;
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> s = SendBufferHelper.Open(4096); // 한번에 큰 덩어리. new byte[4096];  

            //직렬화
            ushort count = 0;
            bool success = true;

            //success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), packet.size);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.packetId);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.playerId);
            count += 8;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), count); // 보내려는 최종 사이즈는 누적된 count.

            if (success == false)
                return null;

            return SendBufferHelper.Close(count);

        }
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }


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
            //역직렬화
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            switch ((PacketID)id)
            {
                case PacketID.PlayerInfoReq:
                    {
                        PlayerInfoReq p = new PlayerInfoReq();
                        p.Read(buffer);
                        Console.WriteLine($"PlayerInfoReq: {p.playerId}");
                    }
                    break;
            }

            Console.WriteLine($"RecvPacketId: {id}, Size: {size}");
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
