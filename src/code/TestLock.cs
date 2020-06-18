
using System.Threading;
using System;
using System.Diagnostics;

internal class TestLock
{
    public static void Main()
    {
        SimpleHybirdLock.Go();
        LazyDemo.Go();
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
}
