using System;
using System.Threading;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Test
{
    public static void Main()
    {
        //AsyncCoordinatorDemo.Go();
        LockFreeStack.Go();
        
    }

    internal static class AsyncCoordinatorDemo
    {
        public static void Go()
        {
            const Int32 timeout = 5000; // Chage to dired timeout.
            MultiWebRequests multiWebRequests = new MultiWebRequests(timeout);
            Console.WriteLine("All operations initiated (Timeout={0}). Hit <Enter> to cancel.",
               (timeout == Timeout.Infinite) ? "Infinite" : (timeout.ToString() + "ms"));
            Console.ReadLine();
            multiWebRequests.Cancel();

            Console.WriteLine();
            Console.WriteLine("Hit enter to terminate.");
            Console.ReadLine();
        }

        public sealed class MultiWebRequests
        {
            private AsyncCoordinator m_ac = new AsyncCoordinator();

            private Dictionary<String, Object> m_servers = new Dictionary<string, object>
        {
            {"http://Wintellect.com/",null },
            {"https://Google.com/",null },
            {"https://1.1.1.1/",null }
        };

            public MultiWebRequests(Int32 timeout = Timeout.Infinite)
            {
                var httpClient = new HttpClient();
                foreach (var server in m_servers.Keys)
                {
                    m_ac.AboutToBegin(1);
                    httpClient.GetByteArrayAsync(server)
                        .ContinueWith(task => ComputeResult(server, task));
                }

                m_ac.AllBegun(AllDone, timeout);
            }

            private void ComputeResult(String server, Task<Byte[]> task)
            {
                Object result;
                if (task.Exception != null)
                {
                    result = task.Exception.InnerException;
                }
                else
                {
                    result = task.Result.Length;
                }
                m_servers[server] = result;
                m_ac.JustEnd();
            }

            public void Cancel() { m_ac.Cancel(); }

            private void AllDone(CoordinationStatus status)
            {
                switch (status)
                {
                    case CoordinationStatus.Cancel:
                        Console.WriteLine("Operation canceled");
                        break;
                    case CoordinationStatus.Timeout:
                        Console.WriteLine("Operation timed-out");
                        break;
                    case CoordinationStatus.AllDone:
                        Console.WriteLine("Operation complete; results below:");
                        foreach (var server in m_servers)
                        {
                            Console.Write("{0} ", server.Key);
                            Object result = server.Value;
                            if (result is Exception)
                            {
                                Console.WriteLine("failed due to {0}.", ((Exception)result).GetType().Name);
                            }
                            else
                            {
                                Console.WriteLine("returned {0:NO} bytes.", result);
                            }
                        }
                        break;
                }
            }
        }



        internal enum CoordinationStatus { AllDone, Timeout, Cancel };

        internal sealed class AsyncCoordinator
        {
            private Int32 m_opCount = 1;
            private Int32 m_statusReported = 0;
            private Action<CoordinationStatus> m_callback;
            private Timer m_timer;

            public void AboutToBegin(Int32 opsToAdd = 1)
            {
                Interlocked.Add(ref m_opCount, opsToAdd);
            }

            // This method must be called after an operations result has been processed.
            public void JustEnd()
            {
                if (Interlocked.Decrement(ref m_opCount) == 0)
                {
                    ReportStatus(CoordinationStatus.AllDone);
                }

            }

            // This method must be called after all operations initialized.
            public void AllBegun(Action<CoordinationStatus> callback,
                Int32 timeout = Timeout.Infinite)
            {
                m_callback = callback;
                if (timeout != Timeout.Infinite)
                {
                    m_timer = new Timer(TimeExpired, null, timeout, Timeout.Infinite);
                }
                JustEnd();
            }

            private void TimeExpired(Object o) { ReportStatus(CoordinationStatus.Timeout); }
            public void Cancel() { ReportStatus(CoordinationStatus.Cancel); }


            // If status has never been reported, report it; else ignore it
            public void ReportStatus(CoordinationStatus status)
            {
                if (Interlocked.Exchange(ref m_statusReported, 1) == 0)
                    m_callback(status);
            }
        }
    }

    internal struct SimpleSpinLock
    {
        private Int32 m_ResourceInUse;  // 0=false, 1=true
        public void Enter()
        {
            while (true)
            {
                if (Interlocked.Exchange(ref m_ResourceInUse, 1) == 0) return;
            }
        }

        public void Leave()
        {
            Volatile.Write(ref m_ResourceInUse, 0);
        }
    }

    public sealed class SomeResource
    {
        private SimpleSpinLock m_sl = new SimpleSpinLock();

        public void AccessResource()
        {
            m_sl.Enter();
            m_sl.Leave();
        }
    }

    internal static class LockFreeStack
    {
        public static void Go()
        {
            lockFreeStack<Int32> lockFreeStack = new lockFreeStack<Int32>();
            lockFreeStack.Push(3);
            lockFreeStack.Push(4);
            lockFreeStack.Push(5);

            Int32 res;
            lockFreeStack.TryPop(out res);
            Console.WriteLine("Pop a item is {0}.", res);
        }
    }

    public class lockFreeStack<T>
    {
        private volatile Node m_head;
        private class Node { public Node Next; public T Value; }

        public void Push(T item)
        {
            var spin = new SpinWait();
            Node node = new Node { Value = item }, head;
            while (true)
            {
                head = m_head;
                node.Next = head;
                if (Interlocked.CompareExchange(ref m_head, node, head) == head)
                    break;
                spin.SpinOnce();
            }
        }

        public bool TryPop(out T result)
        {
            result = default(T);
            var spin = new SpinWait();

            Node head;
            while (true)
            {
                head = m_head;
                if (head == null) return false;
                if(Interlocked.CompareExchange(ref m_head, head.Next, head) == head)
                {
                    result = head.Value;
                    return true;
                }
                spin.SpinOnce();
            }

        }
    }




}

