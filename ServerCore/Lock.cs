using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    // 재귀적 락을 허용할지 (Yes)
    // WriteLock->WriteLock OK / WriteLock->ReadLock OK,
    // ReadLock->WriteLock No
    // 스핀락 정책 (5000번 -> Yield)
    internal class Lock
    {
        const int EMPTY_FLAG = 0x00000000; // (32비트x)
        const int WRITE_MASK = 0x7FFF0000; // (1비트x), 15비트o, (16비트x)
        const int READ_MASK = 0x0000FFFF; // (16비트x), 16비트o
        const int MAX_SPIN_COUNT = 5000;

        // [Unused(1)] [WriteThreadId(15비트)] [ReadCount(16비트)]
        int _flag = EMPTY_FLAG;
        int _writeCount = 0; //재귀적으로 몇개의 락을 할지를 관리.

        public void WriteLock()
        {
            // 특수한 경우. 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThreadId = (_flag & WRITE_MASK) >> 16; // 0x00007FFF
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                _writeCount++;
                return;
            }

            // 아무도 WriteLock or ReadLock을 획득하고 있지 않을 때, 경합해서 소유권을 얻는다
            // 16비트만큼 밀어주니까 총 32비트가 되고, WRITE_MASK를 적용하면, [WriteThreadId(15비트)] 부분만 살아남음.
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    // 시도를 해서 성공하면 return. 아무도 안가지고 있다면 내가 가지고 리턴.
                    //if (_flag == EMPTY_FLAG)
                    //    _flag = desired;
                    // 멀티쓰레드 환경에서는 서로 자기가 성공했다고 하며 동시다발적으로 소유권을 주장할 것이기 때문에
                    // 인터락을 사용해서 하나만 성공하도록 해줘야 함.
                    // _flag값이 EMPTY_FLAG라면, desired값으로 만들어주고, 리턴.
                    // 즉, _flag값이 원래부터 EMPTY_FLAG가 아니라면 조건문을 충족 못해서 리턴 못함.
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                    
                }
                Thread.Yield();
            }
        
        }

        public void WriteUnlock()
        {
            // 잠근만큼 풀어주도록 짝이 맞을 때에만 완전히 나감.
            int lockCount = --_writeCount; 
            if (lockCount == 0)
                // 나갈때는 전부다 EMPTY_FLAG, 즉 0으로 초기화해줌.
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        public void ReadLock() 
        {
            // 특수한 경우. 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThreadId = (_flag & WRITE_MASK) >> 16; // 0x00007FFF
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            // 아무도 WriteLock을 획득하고 있지 않으면, ReadCount를 1 늘린다.
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    //if ((_flag & WRITE_MASK) == 0)
                    //{
                    //    _flag = _flag + 1;
                    //    return;
                    //}
                    int expected = (_flag & READ_MASK); // A(0) B(0). READMASK영역을 제외하고 0으로 다 밀어버린다는 것은 WRITE_MASK에 해당하는 영역이 전부 0인것과 같은 결과.
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected) // A(0->1)가 1로 바꿔놨으니, 다음번에 와서 B(0->1)를 원하는 B는 실행할 수가 없음.
                        return;

                }

                Thread.Yield();
            }
        }  

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag);
        }
    }
}
