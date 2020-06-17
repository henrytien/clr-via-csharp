using System;
using System.Threading;
using System.Threading.Tasks;

public class Example
{
    private static SemaphoreSlim semaphore;
    // A padding interval to make the output more olderly.
    private static int padding;

    public static void Main1()
    {
        // Create the semaphere.
        semaphore = new SemaphoreSlim(0, 3);
        Console.WriteLine("{0} tasks can enter the semaphere.",
            semaphore.CurrentCount);
        Task[] tasks = new Task[5];

        // Create and start five tasks.
        for (int i = 0; i < 5; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                // Each task begines by requiring the semaphore.
                Console.WriteLine("Task {0} begins and waits the semaphore.",
                    Task.CurrentId);

                semaphore.Wait();

                Interlocked.Add(ref padding, 100);

                Console.WriteLine("Task {0} begins enter the semaphore.",Task.CurrentId);

                // The task just sleep for 1 seconds + padding.
                Thread.Sleep(1000 + padding);

                Console.WriteLine("Task {0} release the semaphore; previous count {1}.",
                    Task.CurrentId, semaphore.Release());        
            });

        }
        // Thread sleep half a second, to allow all the tasks to start and block. 
        Thread.Sleep(500);

        // Restore the semaphore count to its maxium value.
        Console.WriteLine("Main thread call Release(3)--->");
        semaphore.Release(3);

        Console.WriteLine("{0} tasks enter the semaphore.",
            semaphore.CurrentCount);

        // Main thread wait all tasks to complete.
        Task.WaitAll(tasks);

    
        Console.WriteLine("henry love mj!");

    }


}