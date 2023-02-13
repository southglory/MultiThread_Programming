using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public enum PacketID
    {
        PlayerInfoReq = 1,
        Test = 2,

    }


    class PlayerInfoReq
    {
        public sbyte testByte;
        public long playerId;
        public string name;

        public class Skill
        {
            public int id;
            public short level;
            public float duration;

            public class Attribute
            {
                public int att;

                public void Read(ReadOnlySpan<byte> s, ref ushort count)
                {
                    this.att = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                    count += sizeof(int);
                }

                public bool Write(Span<byte> s, ref ushort count)//Span: 전체 배열, count: 현재 작업한부분.
                {
                    bool success = true;
                    success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.att);
                    count += sizeof(int);
                    return success;
                }
            }
            public List<Attribute> attributes = new List<Attribute>();


            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {
                this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);
                this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);
                this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);

                // attribute list
                this.attributes.Clear();
                ushort attributeLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
                count += sizeof(ushort);
                for (int i = 0; i < attributeLen; i++)
                {
                    Attribute attribute = new Attribute();
                    attribute.Read(s, ref count);
                    attributes.Add(attribute);
                }

            }

            public bool Write(Span<byte> s, ref ushort count)//Span: 전체 배열, count: 현재 작업한부분.
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
                count += sizeof(float);

                // attribute list
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.attributes.Count);
                count += sizeof(ushort);
                foreach (Attribute attribute in this.attributes)
                    success &= attribute.Write(s, ref count);

                return success;
            }
        }
        public List<Skill> skills = new List<Skill>();


        public void Read(ArraySegment<byte> segment)
        {
            //역직렬화
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
            count += sizeof(ushort);
            count += sizeof(ushort);
            this.testByte = (sbyte)segment.Array[segment.Offset + count];//sbyte일 경우 sbyte로 casting을 위해서 ()추가함.
            count += sizeof(sbyte);
            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;

            // skill list
            this.skills.Clear();
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            for (int i = 0; i < skillLen; i++)
            {
                Skill skill = new Skill();
                skill.Read(s, ref count);
                skills.Add(skill);
            }

        }

        public ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096); // 한번에 큰 덩어리. new byte[4096];  

            //직렬화
            ushort count = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);//slice은 그 자체를 변화시키는 함수가 아니라 계산값을 리턴만 해줌.
            count += sizeof(ushort);
            segment.Array[segment.Offset + count] = (byte)this.testByte;//sbyte일 경우 byte로 강제 casting을 위해서 (byte)추가함.
            count += sizeof(sbyte);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            // skill list
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.skills.Count);
            count += sizeof(ushort);
            foreach (Skill skill in this.skills)
                success &= skill.Write(s, ref count);

            success &= BitConverter.TryWriteBytes(s, count); // 보내려는 최종 사이즈는 누적된 count.
            if (success == false)
                return null;
            return SendBufferHelper.Close(count);
        }
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
                        Console.WriteLine($"PlayerInfoReq: {p.playerId} {p.name}");

                        foreach (PlayerInfoReq.Skill skill in p.skills)
                        {
                            Console.WriteLine($"Skills({skill.id})({skill.level})({skill.duration})");
                        }

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
