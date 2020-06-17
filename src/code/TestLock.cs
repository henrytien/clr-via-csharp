
using System.Threading;
using System;
using System.Diagnostics;

internal class TestLock
{
    public static void Main()
    {
        SimpleHybirdLock.Go();
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
        Stopwatch sw =  Stopwatch.StartNew();
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
        if(Interlocked.Increment(ref m_waiters) == 1)
        {
            return;
        }

        m_waiterLock.WaitOne();
    }

    public void Leave()
    {
        if(Interlocked.Decrement(ref m_waiters) == 0)
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
        if(threadId == m_owningThreadId) { m_recursion++; return; }

        SpinWait spinWait = new SpinWait();
        for(Int32 spinCount = 0; spinCount < m_spincount; spinCount++)
        {
            if (Interlocked.CompareExchange(ref m_waiters, 1, 0) == 0) goto GotLock;

            // Give another threads a chance to run in hope that the lock will be released
            spinWait.SpinOnce();
        }
        // Try one more time.
        if(Interlocked.Increment(ref m_waiters) > 1)
        {
            m_waiterLock.WaitOne();
        }

    GotLock:
        m_owningThreadId = threadId; m_recursion = 1;
    }

    public void Leave()
    {
        Int32 threadId = Thread.CurrentThread.ManagedThreadId;
        if(threadId != m_owningThreadId)
        {
            throw new SynchronizationLockException("Lock not owned by calling thread.");
        }
        // Decrement the recursion count, if this thread still owns the lock, just return
        if (--m_recursion > 0) return;

        m_owningThreadId = 0;

        if(Interlocked.Decrement(ref m_waiters) == 0)
        {
            return;
        }

        m_waiterLock.Set();
    }

    public void Dispose() { m_waiterLock.Dispose(); }
}
