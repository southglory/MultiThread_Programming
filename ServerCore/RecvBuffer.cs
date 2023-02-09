using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class RecvBuffer
    {
        // if it reads Every 8 bytes.(TCP)
        // [rw][ ][ ][ ][ ][ ][ ][ ][ ][ ]
        // [r][ ][ ][ ][ ][w][ ][ ][ ][ ] 5bytes write...
        // [ ][ ][ ][ ][ ][ ][ ][ ][rw][ ] 3bytes write, 8bytes read
        // [rw][ ][ ][ ][ ][ ][ ][ ][ ][ ] reset.

        // [rw][ ][ ][ ][ ][ ][ ][ ][ ][ ]
        // [ ][ ][ ][ ][r][w][ ][ ][ ][ ] 5bytes write. 4bytes read. need more 4bytes read.
        // [r][w][ ][ ][ ][ ][ ][ ][ ][ ] shift. need more 4bytes read.
        // [ ][ ][ ][ ][rw][ ][ ][ ][ ][ ] 3bytes write, 4bytes read.
        // [rw][ ][ ][ ][ ][ ][ ][ ][ ][ ] reset.

        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize { get { return _writePos - _readPos; } }//_readPos커서부터 _writePos커서 이전까지 영역이 Data영역
        public int FreeSize { get { return _buffer.Count - _writePos; } }//_writePos커서를 포함하는 이후 범위가 Free영역.

        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }

        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clean() //버퍼 고갈 전에 커서를 당겨주기.
        {
            int dataSize = DataSize;
            if (dataSize > 0)
            {
                // 남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋.
                _readPos = _writePos = 0;
            }
            else
            {
                // 남은 찌끄레기가 있으면 시작 위치로 복사.
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
                return false;

            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
                return false;

            _writePos += numOfBytes;
            return true;
        }

    }
}
