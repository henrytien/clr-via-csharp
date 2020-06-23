# CLR.via.Csharp
<!-- TOC -->

- [CLR.via.Csharp](#clrviacsharp)
- [Performance](#performance)
  - [Improving performance](#improving-performance)
  - [Hurt perfomance](#hurt-perfomance)
- [Recommend](#recommend)
- [Rules and Guidelines](#rules-and-guidelines)
  - [Define my own classes](#define-my-own-classes)
  - [Declare a type as a value type](#declare-a-type-as-a-value-type)
- [Benefit](#benefit)
  - [Why use generics](#why-use-generics)
- [Restrictions](#restrictions)
  - [Async functions](#async-functions)
- [Difference](#difference)
  - [Value types and reference types](#value-types-and-reference-types)
  - [Sealed class vs unsealed class](#sealed-class-vs-unsealed-class)
- [Tools](#tools)
  - [NGen.exe](#ngenexe)
  - [ILDasm.exe](#ildasmexe)
  - [FxCopCmd.exe](#fxcopcmdexe)
  - [PerfMon.exe](#perfmonexe)
  - [PerfViwe](#perfviwe)
- [FAQ](#faq)
  - [Basic Concepts](#basic-concepts)
  - [Advanced Concepts](#advanced-concepts)
- [Reference](#reference)

<!-- /TOC -->


# Performance

When you learn CSharp language, here are many you should care performance tips. This is a note that helps your code have good performance and avoid hurting performance.

## Improving performance

1. `Unsafe` code is allowed to work directly with memory addresses and can manipulate bytes at these addresses. This is a very powerful feature and is typically useful when interoperating with unmanaged code or when you want to improve the performance of a time-critical algorithm. p17

2. The CLR’s JIT compiler does not have to compile the IL code at run time, and this can improve the application’s performance. 

3. For some applications, the reduction in working set size improves performance, so using NGen can be a net win. P21 

4. Because this programming paradigm is quite common, C# offers a way to simplify this code and improve its performance by providing an as operator. P96

   ```c#
   Employee e = o as Employee; 
   if (e != null) {
      // Use e within the 'if' statement. 
   }
   ```

5. If you have an expression consisting of literals, the compiler is able to evaluate the expression at compile time, improving the application’s performance. P114

6. In some situations, value types can give better performance. In particular, you should declare a type as a value type if all the following statements are true:

   - The type acts as a primitive type. Specifically, this means that it is a fairly simple type that has no members that modify any of its instance fields. 
   - The type doesn’t need to inherit from any other type.
   - The type won’t have any other types derived from it.

7. Auto to have the CLR arrange the fields, it will be improve performance.

   ```c#
   // Let the CLR arrange the fields to improve 
   // performance for this value type. 
   [StructLayout(LayoutKind.Auto)]
   internal struct SomeValType
   {
       private readonly Byte m_b; 
       private readonly Int16 m_x; 
       ...
   }
   ```

8. The generic collection classes offer many improvements over the non-generic equivalents. 

   - One of the biggest improvements is that the generic collection classes allow you to work with collections of value types without requiring that items in the collection be boxed/unboxed. 
   - This in itself greatly improves performance because far fewer objects will be created on the managed heap, thereby reducing the number of garbage collections required by your application.
   - You will get compile-time type safety, and your source code will be cleaner due to fewer casts. 

9. If the `this` and obj arguments refer to the same object, return true. This step can improve performance when comparing objects with many fields. P139

10. Compilers tend to use the call instruction when calling methods defined by a value type because value types are sealed. This implies that there can be no `polymorphism` even for their `virtual` methods, which causes the performance of the call to be faster.

11. Many compilers will never emit code to call a value type’s default constructor automatically, even if the value type offers a parameterless constructor. P186

12. To improve performance and also to avoid considering an extension method that you may not want, the C# compiler requires that you “import” extension methods. P201

13. Be aware that calling a method that takes a variable number of arguments incurs an additional performance hit unless you explicitly pass null. 

14. Because a generic algorithm can now be created to work with a specific value type, the instances of the value type can be passed by value, and the CLR no longer has to do any boxing. 

15. [String pooling](https://docs.microsoft.com/en-us/dotnet/api/system.string.intern?view=netcore-3.1) is another way to improve the performance of strings. P332

16. System.Array implement the generic equivalent of these interfaces, providing better compile-time type safety as well as better performance. P381

17. You might want to consider using an array of arrays (a jagged array) instead of a rectangular array.P386

18. The stack-allocated memory (array) will automatically be freed when the method returns; this is where we get the performance improvement. P388

19. The managed heap allocates these objects next to each other in memory, you get excellent performance when accessing these objects due to locality of reference. P507 

20. The garbage collector improves performance more because it doesn’t traverse every object in the managed heap. P514

21. If you have a lot of free memory, the garbage collector won’t compact the heap; this improves performance but grows your application’s working set. P521

22. When the program needs the data, the program checks the weak reference to see if the object that contains the data is still around, and if it is, the program just uses it; the program experiences high performance. P550 

23. In order to have good performance and compile-time type safety, you want to avoid using reflection as much as possible.  P599 

24. The field contains information that is easily calculated. In this case, you select which fields do not need to be serialized, thus improving your application’s performance by reducing the amount of data transferred. P620 

25. This improves performance significantly, and avoiding context switches is something you want to achieve as often as possible when you design your code. P673 

26. There are some common programming scenarios that can potentially benefit from the improved performance possible with `tasks`. P713 

27. You can potentially improve the performance of this processing by using Parallel LINQ. P717 

28. One of the truly great features of performing asynchronous I/O operations is that you can initiate many of them concurrently so that they are all executing in parallel. This can give your application a phenomenal performance boost. P746 

29. Hybrid constructs provide the performance benefit of the primitive user-mode constructs when there is no thread contention. P789 

30. To get great performance, the `lock` tries to use the Int32 and avoid using the `AutoResetEvent` as much as possible.  P790 

31. The FCL ships with many hybrid constructs that use fancy logic to keep your threads in user mode, improving your application’s performance. P793 

32. For the no­contention case to improve performance and reduce memory consumption. P816 

## Hurt perfomance

1. The size of instances of your type is also a condition to take into account because by default, arguments are passed by value, which causes the fields in value type instances to be copied, hurting performance. P121 

2. Boxing and unboxing/copy operations hurt your application’s performance in terms of both speed and memory. P126 
3. At run time, the Microsoft.CSharp.dll assembly will have to load into the AppDomain, which hurts your application’s performance and increases memory consumption. P148
4. Boxing puts more pressure on the heap, forcing more frequent garbage collections and hurting performance. P166 
5. The CLR would have to verify at each write that the write was not occurring to a constant object, and this would hurt performance significantly. P225
6. You do need to realize that the CLR generates native code for each method the first time the method is called for a particular data type. This will increase an application’s working set size, which will hurt performance. P270 
7. If you perform a lot of string manipulations, you end up creating a lot of String objects on the heap, which causes more frequent garbage collections, thus hurting your application’s performance. P323 
8. Checking strings for equality is a common operation for many applications—this task can hurt performance significantly. It is good way use  function `Object.ReferenceEquals`P329 
9. Dynamically growing the array hurts performance; avoid this by setting a good initial capacity. P337
10. If you construct `ASCIIEncoding` objects yourself, you are creating more objects on the heap, which hurts your application’s performance.P353
11. Allocating short-lived large objects will cause generation 2 to be col- lected more frequently, hurting performance. P520 
12. Invoking a member by using reflection will also hurt performance. P590 
13. A performance hit occurs when Windows context switches to another thread. P673
14. if you have more threads than CPUs, then context switching is introduced and performance dete- riorates. P674 
15. Calling `Wait` or querying a task’s Result property when the task has not yet finished running will most likely cause the thread pool to create a new thread, which increases resource usage and hurts performance. P705 
16. Thread synchronization would hurt performance, not requiring thread synchronization is good. P716 
17. This thread synchronization lock can become a bottleneck in some applications, thereby limiting scalability and performance to some degree. P724 
18. Tasks are stolen from the tail of a local queue and require that a thread synchronization lock be taken, which hurts performance a little bit. P725
19. `Locks` is that they hurt performance. P756 
20. Having threads transition from user mode to kernel mode and back incurs a big performance hit, which is why kernel-mode constructs should be avoided. P761 
21. Each method call on a kernel object causes the calling thread to transition from managed code to native user-mode code to native kernel-mode code and then return all the way back. These transitions require a lot of CPU time and, if performed frequently, can adversely affect the overall performance of your application. P778 
22. Calling `WaitOne` causes the thread to transition into the Windows’ kernel, and this is a big performance hit.  P791 
23. Entering and leaving a `try` block decreases the performance of the method. P799 
24. Avoid using recursive locks (especially recursive reader-writer locks) because they hurt performance. P806 

# Recommend

1. If you have multiple types that can share a single version number and security settings, it is recommended that you place all of the types in a single file rather than spread the types out over separate files, let alone separate assemblies.  p46
2. When designing a type, you should try to minimize the number of virtual methods you define.
   - First, Calling a virtual method is slower than calling a nonvirtual method. 
   - Second, virtual methods cannot be inlined by the JIT compiler, which further hurts performance. 
   - Third, virtual methods make versioning of components more brittle.
   - Fourth, when defining a base type, it is common to offer a set of convenience overloaded methods. 
3. When normalizing strings, it is highly recommended that you use ToUpper­ Invariant instead of ToLowerInvariant because Microsoft has optimized the code for performing uppercase comparisons. P325
4. I’d highly recommend against performing synchronous I/O operations in a server application. P719 

# Rules and Guidelines

## Define my own classes

- I will simulate creating a closed class by using the above technique of sealing the virtual methods that my class inherits.
-  I always define my data fields as private and I never waver on this. 
- I always define my methods, properties, and events as private and nonvir- tual. 

## Declare a type as a value type

-  Instances of the type are small (approximately 16 bytes or less).

-  Instances of the type are large (greater than 16 bytes) and are not passed as method param- eters 		or returned from methods.



# Benefit

## Why use generics

- Source code protection
- Type safety
- Clear source
- Better performance

# Restrictions

## Async functions

- You cannot turn your application's `Main` method into an `async` function. 
- You cannot have any `out` or `ref` parameters on an `async` function. 
- You cannot use the `await` operator inside a `catch`, `finally`, or `unsafe` block.
- You cannot take a lock that supports thread ownership or recurcion before an `await` operator and release it after the `await` operator.
- Within a query expression, the `await` operator may only be used within the first collection expression. P744 


# Difference

## Value types and reference types

- Value type objects have two representations: an unboxed form and a boxed form,Reference types are always in a boxed form.
- Value types are derived from System.ValueType.
- You can’t define a new value type or a new reference type by using a value type as a base class, you shouldn’t introduce any new virtual methods into a value type. No methods can be abstract, and all methods are implicitly sealed (can’t be overridden).
- When you assign a value type variable to another value type variable, a field-by-field copy is made. When you assign a reference type variable to another reference type variable, only the memory address is copied.
- Two or more reference type variables can refer to a single object in the heap, allowing operations on one variable to affect the object referenced by the other variable. On the other hand, value type variables are distinct objects, and it’s not possible for operations on one value type variable to affect another.
- Because unboxed value types aren’t allocated on the heap, the storage allocated for them is freed as soon as the method that defines an instance of the type is no longer active as opposed to waiting for a garbage collection.

## Sealed class vs unsealed class

- **Versioning**

- **Performance**

- **Security and predictability**

# Tools

## NGen.exe

The NGen.exe tool that ships with the .NET Framework can be used to compile IL code to native code when an application is installed on a user’s machine.

- **Improving an application’s startup time**
- **Reducing an application’s working set**

## ILDasm.exe

I always use a tool such as ILDasm.exe to view the IL code for my methods and see where the box IL instructions are. P127

## FxCopCmd.exe

Enforced by the Code Analysis tool (FxCopCmd.exe) in Visual Studio.

## PerfMon.exe

The easiest way to access the System Monitor control is to run PerfMon.exe.

## PerfViwe

[PerfView](https://github.com/microsoft/perfview) is a free performance-analysis tool that helps isolate CPU and memory-related performance issues.



# FAQ

## Basic Concepts

1. **What is an Object and a Class?**
2. **What are the fundamental OOP concepts?**
3. **What is Managed and Unmanaged code?**
4. **What is an Interface?**
5. **What are the different types of classes in C#?**
6. **Explain code compilation in C#.**
7. **What are the differences between a Class and Struct?**
8. **What is the difference between the Virtual method and Abstract method?**
9. **Explain Namespaces in C#.**
10. **What is "using" statement in C#?**
11. **Explain Abstraction.** 
12. **Explain Polymorphism?**
13. **How is Exception Handing implemented in C#?**  
14. **What are C# I/O Classes? What are the common used I/O classes?**
15. **What is StreamReader/StreamWriter classes?**
16. **What is a Destructor in C#?**
17. **What is an Abstract Class?**
18. **What are Boxing and Unboxing?**
19. **What is the difference between Continue and Break Statement?**
20. **What is difference between finaly and finalize block?**
21. **What is an Array? Give the syntax for a single and multi-dimensional array?**
22. **What is a Jagged Array?**
23. **Name some properties of Array.**
24. **What is an Array Class?**
25. **What is a String? What are the properties of a String Class?** 
26. **What is an Escape Sequence? Name some String escape sequences in C#**
27. **What are Regular expressions? Search a string using regular expressions?**
28. **What are the basic string Operations?**
29. **What is Parsing? How to Parse a Date Time String?**
30. **Can we use "this" command within a static method?**
31. **What is difference between constaints and read-only? **

## Advanced Concepts

1. **What is a Delegate?**
31. **What are Events?**
32. **How to use Delegates with Events?**
33. **What are the different types of Delegates?**
34. **What do Multicast Delegates mean?**
35. **Explain Publisher and Subscribers in Events.**
36. **What are Synchronization and Asynchronous operations?**
37. **What is Reflection in C#?**
38. **What is a Generic Class?**
39. **Explain Get and Set Accessor properties?**
40. **What is a Thread? What is a MultiThreading?**
41. **Name some properties of the Thread.**
42. **What a the different states of a Thread?**
43. **What a Async and Await?**
44. **What is a Deadlock?**
45. **Explain Locks, Monitors, and Mutex Object in Threading.**
46. **What is Race Condition?**
47. **What is Thread Pooling?**
48. **What is Serialization?**
49. **What are the types of Serialization?** 
21. [Top 10 C# .NET Multithreading Interview Questions](http://dotnetpattern.com/multi-threading-interview-questions)

[Answers are here](https://www.softwaretestinghelp.com/c-sharp-interview-questions/) 



## Recommendations

- [Managed-threading-best-practices](https://docs.microsoft.com/en-us/dotnet/standard/threading/managed-threading-best-practices#general-recommendations) 
- [Thousands of Threads and Blocking I/O](https://www.slideshare.net/e456/tyma-paulmultithreaded1) 
- [The C10K problem](http://www.kegel.com/c10k.html)
- [IO Processing](https://flylib.com/books/en/4.491.1.85/1/) 
- [High-Performance Server Architecture](http://pl.atyp.us/content/tech/servers.html) 
- [Understanding reactor pattern with java](http://kasunpanorama.blogspot.com/2015/04/understanding-reactor-pattern-with-java.html)

## StackOverflow Questions

- [C++ Vs. C# - What’s the Difference?](https://www.guru99.com/cpp-vs-c-sharp.html)
- [Difference between semaphore and semaphoreslim?](https://docs.microsoft.com/en-us/dotnet/standard/threading/semaphore-and-semaphoreslim)
- [SpinWait vs Sleep waiting. Which one to use?](https://stackoverflow.com/questions/9719003/spinwait-vs-sleep-waiting-which-one-to-use) 
- [Need to understand the usage of SemaphoreSlim?](https://stackoverflow.com/questions/20056727/need-to-understand-the-usage-of-semaphoreslim) 
- [How to “sleep” until timeout or cancellation is requested in .NET 4.0](https://stackoverflow.com/questions/18715099/how-to-sleep-until-timeout-or-cancellation-is-requested-in-net-4-0)
- [Difference between decimal, float and double in .NET?](https://stackoverflow.com/questions/618535/difference-between-decimal-float-and-double-in-net)  
- [What's the purpose of Thread.SpinWait method?](https://stackoverflow.com/questions/1091135/whats-the-purpose-of-thread-spinwait-method)
- [What is a semaphore?](https://stackoverflow.com/questions/34519/what-is-a-semaphore) 
- [Lock, mutex, semaphor,  what's the difference?](https://stackoverflow.com/questions/2332765/lock-mutex-semaphore-whats-the-difference) 
- [Semaphore vs. Monitors - what's the difference?](https://stackoverflow.com/questions/7335950/semaphore-vs-monitors-whats-the-difference) 
- [Difference between wait() and sleep()?](https://stackoverflow.com/questions/1036754/difference-between-wait-and-sleep)
- [How to check possibility of deadlock in c# code](https://stackoverflow.com/questions/54001297/how-to-check-possibility-of-deadlock-in-c-sharp-code)
- [Multiple awaits vs Task.WaitAll - equivalent?](https://stackoverflow.com/questions/32119507/multiple-awaits-vs-task-waitall-equivalent)
- [Await on a completed task same as task.Result?](https://stackoverflow.com/questions/24623120/await-on-a-completed-task-same-as-task-result) 
- [ConcurrentBag and lock(List) which is faster to add or remove?](https://stackoverflow.com/questions/29307035/concurrentbagt-and-locklistt-which-is-faster-to-add-or-remove)
- [No ConcurrentList in .Net 4.0?](https://stackoverflow.com/questions/6601611/no-concurrentlistt-in-net-4-0)
- [What's the difference between Invoke() and BeginInvoke()?](https://stackoverflow.com/questions/229554/whats-the-difference-between-invoke-and-begininvoke)

## Awesome Questions

- [Task vs. TaskCompletionSource in C#](https://www.pluralsight.com/guides/task-taskcompletion-source-csharp) 
- [Is the C# static constructor thread safe?](https://stackoverflow.com/questions/7095/is-the-c-sharp-static-constructor-thread-safe) 
- [Volatile vs. Interlocked vs. lock](https://stackoverflow.com/questions/154551/volatile-vs-interlocked-vs-lock)

# References

- [awesome-dotnet](https://github.com/quozd/awesome-dotnet) 

  A collection of awesome .NET libraries, tools, frameworks, and software.

- [C# documentation](https://docs.microsoft.com/en-us/dotnet/csharp/)

  Learn how to write any application using the C# programming language on the .NET platform.
  
- [StackOverflow](https://stackoverflow.com/)







