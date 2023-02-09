
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    class Knight
    {
        public int hp;
        public int attack;
        public int name;
        public List<int> skills = new List<int>();
    }

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            Knight knight = new Knight() { hp = 100, attack = 10 };

            // [ ][ ][ ][ ][ ][ ][ ][ ]
            // [    100   ][    10    ]
            // hp, attack에 대해서 각각 만들고 있는 부분은 나중에 serialize해서? 자동으로 하도록 바꾸게 될 것임..
            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096); // 한번에 큰 덩어리. new byte[4096];  
            byte[] buffer = BitConverter.GetBytes(knight.hp);
            byte[] buffer2 = BitConverter.GetBytes(knight.attack);
            Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);


            // 이동패킷 100명
            // Session외부에 두었기 때문에 1개 -> 1 * 100명
            // Session내부에 두었다면 100개 세션 -> 100 * 100명 (= 1만개). 비효율.
            Send(sendBuff);
            Thread.Sleep(1000);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");

        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
    class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            // www.rookiss.com => 123.123.123.12
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);


            _listener.Init(endPoint, () => { return new GameSession(); });//어떤 세션을? 게임세션을 만들어주세요 라고 서버에게 요청.
            Console.WriteLine("Listening...");

            while (true)
            {
                ;
            }
        }
    }
}