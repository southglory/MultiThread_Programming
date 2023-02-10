using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
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
        public string name;

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> segment)
        {
            //역직렬화
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            count += sizeof(ushort);
            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count)); // count만 변조하는 악의적인 공격을 감지하기 위해서 누적count보다 header리턴하는 count가 작으면 에러가 나도록 수정함.(READONLY SPAN)
            count += sizeof(long);

            // string
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096); // 한번에 큰 덩어리. new byte[4096];  

            //직렬화
            ushort count = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetId);//slice은 그 자체를 변화시키는 함수가 아니라 계산값을 리턴만 해줌.
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);

            // string
            ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, count, nameLen);
            count += nameLen;

            success &= BitConverter.TryWriteBytes(s, count); // 보내려는 최종 사이즈는 누적된 count.

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

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001, name = "ABCD" };//보내려는 최종 size는 미리 알수 없으므로 지웠음.

            // 보낸다(손님이 먼저 보냄
            //for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> s = packet.Write(); // Serialize한 결과를 byte배열로 받음.
                if (s != null)
                    Send(s);

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
