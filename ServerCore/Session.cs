using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{

    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0; // 끊겼는지 여부를 관리.

        RecvBuffer _recvBuffer = new RecvBuffer(1024);

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue= new Queue<ArraySegment<byte>>();
        List < ArraySegment<byte>> _pendingList = new List < ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes) ;
        public abstract void OnDisconnected(EndPoint endPoint) ;

        public void Start(Socket socket)
        {
            _socket= socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            RegisterRecv();            
        }

        public void Send(ArraySegment<byte> sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) // 이미 disconnected가 되었었다면(두번 disconnect시도한거라면) 리턴.
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            //BufferList사용법: ArraySegment list를 만든 '다음에' 넣어줘야 함.

            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff); //struct구조체이고 클래스가 아니므로 heap이 아니라 stack에 할당됨.
            }
            _sendArgs.BufferList = _pendingList;

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        if (_sendQueue.Count > 0)
                            RegisterSend();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        void RegisterRecv()
        {
            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            bool pending = _socket.ReceiveAsync(_recvArgs); //pending: 보류중
            if (pending == false)
                OnRecvCompleted(null, _recvArgs);
        }


        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                //TODO
                try
                {
                    // Write 커서 이동
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다.
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }

                    // Read 커서 이동
                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch(Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }
            else
            {

                // TODO Disconnect
                Disconnect();
            }
        }

        #endregion
    }
}
