using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;



public class MutexTest
{
    public static void Main2()
    {
        //SomeClass.Go();
        Task task = Task.Run(() => SomeClass.Go());
        Console.WriteLine("Hello world.");
    }
}

internal class SomeClass 
{
    public static void Go()
    {
        Method1();
        //m_lock.ReleaseMutex(); // Exception: Mutex is not own.
    }


    private readonly static Mutex m_lock = new Mutex();

    public static void  Method1()
    {
        m_lock.WaitOne();
        Method2();
        m_lock.ReleaseMutex();
    }

    public static void Method2()
    {
        m_lock.WaitOne();
        Console.WriteLine("Method2 get mutex!");
        m_lock.ReleaseMutex();
    }

    public void Dispose() { m_lock.Dispose(); }
}


internal sealed class RecursiveAutoResetEventTest
{
    public void Go()
    {
        Enter();
    }
    // This class is effective than SomeClass, becuase the thread own and recursion code are
    // manage code. Only first get a AutoResetEvent or give up the last one.
    private AutoResetEvent m_lock = new AutoResetEvent(true);
    private Int32 m_owningThreadId = 0;
    private Int32 m_orecurisonCount = 0;

    public  void Enter()
    {
        Int32 currentThreadId = Thread.CurrentThread.ManagedThreadId;

        if(m_owningThreadId == currentThreadId)
        {
            m_orecurisonCount++;
            return;
        }

        // The calling thread doesn't have own lock, wait it 
        m_lock.WaitOne();

        // The calling thead now own the lock, intialize the owning thread ID and recursion count
        m_owningThreadId = currentThreadId;
        m_orecurisonCount--;
    }

    public void Leave()
    {
        if(m_owningThreadId != Thread.CurrentThread.ManagedThreadId)
        {
            throw new InvalidCastException();
        }

        // Subtract 1 from recurison count
        if(-- m_orecurisonCount == 0)
        {
            // If the recursion count is zero, then no thread owns the lock
            m_owningThreadId = 0;
            m_lock.Set();  // Wait up one thead, if have
        }
    }

    public void Dispose() { m_lock.Dispose(); }

}



