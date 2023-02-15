using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });// 컨텐츠끼리의 경합을 막기 위해서 ThreadLocal 타입으로 만들어줌.

        public static int ChunkSize { get; set; } = 65535 * 100; // 큰 덩어리 사이즈.
        
        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (CurrentBuffer.Value == null) //한번도 사용 안했으니까 만들어줌.
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            if (CurrentBuffer.Value.FreeSize < reserveSize) // 있기는 한데 필요한사이즈보다 작으면 기존chunk를 날리고 새로운 chunk로 교체.
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            return CurrentBuffer.Value.Open(reserveSize); // chunk가 있기도 하고 남아있는 크기도 충분하니까 필요한사이즈의 버퍼만 리턴. 
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }
    public class SendBuffer
    {
        // [u][ ][ ][ ][ ][ ][ ][ ][ ][ ]
        byte[] _buffer;
        int _usedSize = 0;

        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }

        public ArraySegment<byte> Open(int reserveSize)//버퍼를 오픈하면서 공간을 예약.
        {
            if (reserveSize > FreeSize)
                return null;

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize) 
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize; //실제로 사용한 버퍼사이즈 반환.
            return segment;
        }
    }
}
