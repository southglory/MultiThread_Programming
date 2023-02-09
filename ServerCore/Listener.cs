using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory;//세션을 어떤 방식으로 누구를 만들지 정의하는 함수.


        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            // 문지기 교육. 정문이 여기다.
            _listenSocket.Bind(endPoint);

            // 영업 시작
            // backing : 최대 대기수
            _listenSocket.Listen(10);

            for (int i = 0; i < 10; i++) // 동접자가 많으면 이렇게 늘려도 된다.
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                RegisterAccept(args);//낚시대를 던진다.
            }
        }

        void RegisterAccept(SocketAsyncEventArgs args)//예약
        {
            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)//던지자마자 잡혀서 올렸다.
                OnAcceptCompleted(null, args);
        }


        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                //TODO
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
                Console.WriteLine(args.SocketError.ToString());

            RegisterAccept(args);//다시 낚시대를 던진다.
        }

    }
}
