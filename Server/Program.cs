
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    
    class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            // 멀티쓰레드가 개입을 하지 않는 맨 처음 싱글쓰레드 부분에 PacketManager의 Register()함수를 호출.
            PacketManager.Instance.Register();

            // DNS (Domain Name System)
            // www.rookiss.com => 123.123.123.12
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);


            _listener.Init(endPoint, () => { return new ClientSession(); });//어떤 세션을? Client세션을 만들어주세요 라고 서버에게 요청.
            Console.WriteLine("Listening...");

            while (true)
            {
                ;
            }
        }
    }
}