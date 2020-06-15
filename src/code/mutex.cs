using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;


public class MutexTest
{
    public static void Main()
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




