
using System.Threading;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;


internal class TestLock
{
    public static void Main1()
    {
        //SimpleHybirdLock.Go();
        //LazyDemo.Go();
        //LazyDemo.Go1();
        //SynchronizedQueueTest<Int32>.Go();
        //ConsumerModel.Go();
    }
}

internal sealed class SimpleHybirdLock : IDisposable
{
    public static void Go()
    {
        Int32 x = 0;
        const Int32 iterations = 1000000; // about 14ms for ++

        SimpleHybirdLock hybirdLock = new SimpleHybirdLock();

        hybirdLock.Enter();
        x++;
        hybirdLock.Leave();
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            hybirdLock.Enter();
            i++;
            hybirdLock.Leave();
        }
        //hybirdLock.Leave();
        Console.WriteLine("Incrementing x in SimpleHybridLock: {0:N0}", sw.ElapsedMilliseconds);
    }
    private Int32 m_waiters = 0;

    private AutoResetEvent m_waiterLock = new AutoResetEvent(false);

    public void Enter()
    {
        if (Interlocked.Increment(ref m_waiters) == 1)
        {
            return;
        }

        m_waiterLock.WaitOne();
    }

    public void Leave()
    {
        if (Interlocked.Decrement(ref m_waiters) == 0)
        {
            return;
        }

        m_waiterLock.Set(); // Wake up one thread from blokc thread.
    }
    // Don't forget call 
    public void Dispose() { m_waiterLock.Dispose(); }
}

internal sealed class AnotherHybridLock : IDisposable
{
    private Int32 m_waiters = 0;

    private AutoResetEvent m_waiterLock = new AutoResetEvent(false);

    private Int32 m_spincount = 5200; // Arbitrarily chose count.

    private Int32 m_owningThreadId = 0, m_recursion = 0;

    public void Enter()
    {
        Int32 threadId = Thread.CurrentThread.ManagedThreadId;
        if (threadId == m_owningThreadId) { m_recursion++; return; }

        SpinWait spinWait = new SpinWait();
        for (Int32 spinCount = 0; spinCount < m_spincount; spinCount++)
        {
            if (Interlocked.CompareExchange(ref m_waiters, 1, 0) == 0) goto GotLock;

            // Give another threads a chance to run in hope that the lock will be released
            spinWait.SpinOnce();
        }
        // Try one more time.
        if (Interlocked.Increment(ref m_waiters) > 1)
        {
            m_waiterLock.WaitOne();
        }

    GotLock:
        m_owningThreadId = threadId; m_recursion = 1;
    }

    public void Leave()
    {
        Int32 threadId = Thread.CurrentThread.ManagedThreadId;
        if (threadId != m_owningThreadId)
        {
            throw new SynchronizationLockException("Lock not owned by calling thread.");
        }
        // Decrement the recursion count, if this thread still owns the lock, just return
        if (--m_recursion > 0) return;

        m_owningThreadId = 0;

        if (Interlocked.Decrement(ref m_waiters) == 0)
        {
            return;
        }

        m_waiterLock.Set();
    }

    public void Dispose() { m_waiterLock.Dispose(); }
}

internal sealed class Transaction
{
    private DateTime m_timeOfLastTrans;

    public void PerformTransaction()
    {
        Monitor.Enter(this);

        m_timeOfLastTrans = DateTime.Now;
        Monitor.Exit(this);
    }

    public DateTime LastTransaction
    {
        get
        {
            Monitor.Enter(this);
            // This code has shared access the data 
            DateTime temp = m_timeOfLastTrans;
            Monitor.Exit(this);
            return temp;
        }
    }

    public static void SomeMethod()
    {
        var t = new Transaction();
        Monitor.Enter(t);
        // ThreadPool will block until the thread call Monitor.Exit.
        ThreadPool.QueueUserWorkItem(o => Console.WriteLine(t.LastTransaction));
        Monitor.Exit(t);
    }


    internal sealed class TransactionPrivaeLock
    {
        private readonly Object m_lock = new object();
        private DateTime m_timeOfLastTrans;

        public void PerformTransaction()
        {
            Monitor.Enter(m_lock);
            m_timeOfLastTrans = DateTime.Now;
            Monitor.Exit(m_lock);
        }

        public DateTime LastTransaction
        {
            get
            {
                Monitor.Enter(m_lock);
                DateTime temp = m_timeOfLastTrans;
                Monitor.Exit(m_lock);
                return temp;
            }
        }

        private void SomeMethod()
        {
            lock (this)
            {

            }
        }

        private void SomeMethod1()
        {
            Boolean lockTaken = false;
            try
            {
                Monitor.Enter(this, ref lockTaken);
            }
            finally
            {
                if (lockTaken) Monitor.Exit(this);
            }
        }

        internal sealed class Transaction : IDisposable
        {
            private readonly ReaderWriterLockSlim m_lock =
                new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            private DateTime m_timeOfLastTrans;

            public void PerformTransaction()
            {
                m_lock.EnterWriteLock();
                m_timeOfLastTrans = DateTime.Now;
                m_lock.ExitWriteLock();
            }

            public DateTime LastTransaction
            {
                get
                {
                    m_lock.EnterReadLock();
                    DateTime temp = m_timeOfLastTrans;
                    m_lock.ExitWriteLock();
                    return temp;
                }
            }

            public void Dispose() { m_lock.Dispose(); }
        }
    }
}

// 30.4 Double-Check Locking
public sealed class Singleton
{
    private static Object s_lock = new object();

    private static Singleton s_value = null;

    private Singleton()
    {
        // Initialize code 
    }

    public static Singleton GetSingleton()
    {
        if (s_value != null) return s_value;

        Monitor.Enter(s_lock);
        if (s_value == null)
        {
            Singleton temp = new Singleton();
            Volatile.Write(ref s_value, temp);
        }
        Monitor.Exit(s_lock);
        return s_value;
    }
}

internal sealed class SingletonV2
{
    private static SingletonV2 s_value = new SingletonV2();

    private SingletonV2()
    {

    }

    public static SingletonV2 GetSingletonV2() { return s_value; }
}

internal sealed class SingletonV3
{
    private static SingletonV3 s_value = null;
    private SingletonV3() { }

    public static SingletonV3 GetSingletonV3()
    {
        if (s_value != null) return s_value;
        SingletonV3 temp = new SingletonV3();
        Interlocked.CompareExchange(ref s_value, temp, null);

        return s_value;
    }
}
internal sealed class LazyDemo
{
    public static void Go()
    {
        Lazy<String> s = new Lazy<string>(() =>
        DateTime.Now.ToLongTimeString(), true);

        Console.WriteLine(s.IsValueCreated);
        Console.WriteLine(s.Value);
        Console.WriteLine(s.IsValueCreated);
        Thread.Sleep(1000);
        Console.WriteLine(s.Value);
    }

    public static void Go1()
    {
        string name = null;
        LazyInitializer.EnsureInitialized(ref name, () => "Henry");

        Console.WriteLine(name);

        LazyInitializer.EnsureInitialized(ref name, () => "Mj");
        string saying = null;
        LazyInitializer.EnsureInitialized(ref saying, () => "I love mj");
        Console.WriteLine(saying);
    }
}

public sealed class ConditionVariablePattern
{
    private readonly Object m_lock = new object();
    private Boolean m_condition = false;

    public void Thread1()
    {
        Monitor.Enter(m_lock);

        while (!m_condition)
        {
            Monitor.Wait(m_lock); // Temporarily release lock so other thread can change it.
        }

        Monitor.Exit(m_lock);
    }

    public void Thread2()
    {
        Monitor.Enter(m_lock);

        m_condition = true;
        // Monitor.Pulse(m_lock);
        Monitor.PulseAll(m_lock); // Wake all waiter after lock is released.

        Monitor.Exit(m_lock);
    }
}

internal sealed class SynchronizedQueueTest<T>
{
    private readonly Object m_lock = new Object();
    private readonly Queue<T> m_queue = new Queue<T>();

    public void Enqueue(T item)
    {
        Monitor.Enter(m_lock);

        m_queue.Enqueue(item);
        Monitor.PulseAll(m_lock);
        Monitor.Exit(m_lock);
    }

    public T Dequeue()
    {
        Monitor.Enter(m_lock);
        while (m_queue.Count == 0)
        {
            Monitor.Wait(m_lock);
        }

        T item = m_queue.Dequeue();
        Monitor.Exit(m_lock);
        return item;
    }

    public Int32 Count()
    {
        return m_queue.Count;
    }

    public static void Go()
    {
        var queue = new SynchronizedQueueTest<Int32>();
        queue.Enqueue(520);
        queue.Enqueue(520);
        queue.Enqueue(521);

        Console.WriteLine("Dequeue {0} , Count {1}.", queue.Dequeue(), queue.Count());
    }
}

public enum OneManyMode { Exclusive, Shared }
public sealed class AsyncOneManyLock
{
    private SpinLock m_lock = new SpinLock(true);
    private void Lock() { Boolean taken = false; m_lock.Enter(ref taken); }
    private void Unlock() { m_lock.Exit(); }


    private Int32 m_state = 0;
    private Boolean IsFree { get { return m_state == 0; } }
    private Boolean IsOwnedByWriter { get { return m_state == -1; } }
    private Boolean IsOwnedByReaders { get { return m_state > 0; } }
    private Int32 AddReaders(Int32 count) { return m_state += count; }
    private Int32 SubractReader() { return --m_state; }
    private void MakeWriter() { m_state = -1; }
    private void MakeFree() { m_state = 0; }

    private readonly Task m_noContentionAccessGranter;
    // Writer 
    private readonly Queue<TaskCompletionSource<Object>> m_qWaitingWriters =
        new Queue<TaskCompletionSource<object>>();
    // Reader
    private TaskCompletionSource<Object> m_waitingReadersSignal =
        new TaskCompletionSource<object>();

    private Int32 m_numWaitingReader = 0;

    public AsyncOneManyLock()
    {
        m_noContentionAccessGranter = Task.FromResult<Object>(null);
    }

    public Task WaitAsync(OneManyMode mode)
    {
        Task accressGranter = m_noContentionAccessGranter;

        Lock();

        switch (mode)
        {
            case OneManyMode.Exclusive:
                if (IsFree)
                {
                    MakeWriter();
                }
                else
                {
                    var tcs = new TaskCompletionSource<Object>();
                    m_qWaitingWriters.Enqueue(tcs);
                    accressGranter = tcs.Task;
                }
                break;
            case OneManyMode.Shared:
                if (IsFree || (IsOwnedByReaders && m_qWaitingWriters.Count == 0))
                {
                    AddReaders(1);
                }
                else
                {
                    m_numWaitingReader++;
                    accressGranter = m_waitingReadersSignal.Task.ContinueWith(t => t.Result);
                }
                break;
        }
        Unlock();
        return accressGranter;
    }

    public void Release()
    {
        TaskCompletionSource<Object> accessGranter = null;
        Lock();
        if (IsOwnedByWriter) MakeFree();
        else { SubractReader(); }

        if (IsFree)
        {
            if (m_qWaitingWriters.Count > 0)
            {
                MakeWriter();
                accessGranter = m_qWaitingWriters.Dequeue();
            }
            else if (m_numWaitingReader > 0)
            {
                AddReaders(m_numWaitingReader);
                m_numWaitingReader = 0;
                accessGranter = m_waitingReadersSignal;
                m_waitingReadersSignal = new TaskCompletionSource<object>();
            }
        }
        Unlock();
        if (accessGranter != null) accessGranter.SetResult(null);
    }

    private static async Task AccessResourceViaAsyncSynchronization(
        AsyncOneManyLock asyncLock)
    {
        //await asyncLock.AcquireAsync(OneManyMode.Shared);

        asyncLock.Release();
    }
}

internal sealed class ConsumerModel
{
    public static void Go()
    {
        var b1 = new BlockingCollection<Int32>(new ConcurrentQueue<Int32>());

        ThreadPool.QueueUserWorkItem(ConsumeItems, b1);

        for(Int32 item = 0; item < 5; item++)
        {
            Console.WriteLine("Producting: " + item);
            b1.Add(item);
        }

        b1.CompleteAdding();
        Console.ReadLine();
    }

    private static void ConsumeItems(Object o)
    {
        var b1 = (BlockingCollection<Int32>)o;
        foreach( var item in b1.GetConsumingEnumerable())
        {
            Console.WriteLine("Consuming: " + item);

        }
        Console.WriteLine("All items have been consumed.");
    }
}

namespace NightClub
{
    public class Program
    {
        public static Semaphore Bouncer { get; set; }

        public static void Main(string[] args)
        {
            // Create 3 semaphore of with 3 slots, where 3 are availiable.
            Bouncer = new Semaphore(3, 3);

            // Open the nightclub
            OpenNightClub();
        }

        public static void OpenNightClub()
        {
            for (int i = 0; i < 50; i++)
            {
                // Let each guets enter each thread.
                Thread thread = new Thread(new ParameterizedThreadStart(Guest));
                thread.Start(i);
            }
        }

        public static void Guest(Object args)
        {
            // Wait to enter the nightclub(a semaphore will be realeased.)
            Console.WriteLine("Guest {0} is waitting to entering the nightclub.", args);
            Bouncer.WaitOne();

            // Doing some dancing
            Console.WriteLine("Guest {0} is doing some dancing.", args);
            Thread.Sleep(500);

            // Let one guest out(release one semaphore)
            Console.WriteLine("Guest {0} is leaving the nightclub.", args);
            Bouncer.Release();
        }
    }
}
