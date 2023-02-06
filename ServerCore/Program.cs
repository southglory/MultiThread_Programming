using System;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
   
    class Program
    {
        static Listener _listener = new Listener();

        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                // 받는다
                byte[] recvBuff = new byte[1024];
                int recvBytes = clientSocket.Receive(recvBuff);//데이터는 recvBuff에 넣어주고, 몇 바이트를 받았는지는 recvBytes에 넣어준다.
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes); //시작인덱스0
                Console.WriteLine($"[From Client] {recvData}");

                // 보낸다
                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG server !");
                clientSocket.Send(sendBuff); //보내려는데 안 받으면 계속 대기한다.

                // 쫒아낸다
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch(Exception e) 
            {
                Console.WriteLine(e);
            }
            
        }
        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            // www.rookiss.com => 123.123.123.12
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);


            _listener.Init(endPoint, OnAcceptHandler);
            Console.WriteLine("Listening...");

            while (true)
            {
                ;
            }
        }
    }
}