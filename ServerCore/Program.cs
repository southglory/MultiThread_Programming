using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    // 전역(static공간)에 있는 [ JobQueue ] 에 락을 걸어서 접근한 다음 한번에 최대한 많은 일감을 빼오면, 락을 하는 횟수를 줄일 수 있다.
    class Program
    {
        // TLS공간 사용하기: ThreadLocal
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(() => { return $"My Name is {Thread.CurrentThread.ManagedThreadId}"; });

        static void WhoAmI()
        {
            bool repeat = ThreadName.IsValueCreated;
            if (repeat)
                Console.WriteLine(ThreadName.Value + "(repeat)");
            else
                Console.WriteLine(ThreadName.Value);
        }

        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);

            ThreadName.Dispose();
        }
    }
}