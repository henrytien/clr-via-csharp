# CLR.via.Csharp



# Performance



## Improving performance

1. **Unsafe** code is allowed to work directly with memory addresses and can manipulate bytes at these addresses. This is a very power- ful feature and is typically useful when interoperating with unmanaged code or when you want to improve the performance of a time-critical algorithm. p17

2. The CLR’s JIT compiler does not have to compile the IL code at run time, and this can improve the applica- tion’s performance. 

3. For some applications, the reduction in working set size improves performance, so using NGen can be a net win. P21 

4. Because this programming paradigm is quite com- mon, C# offers a way to simplify this code and improve its performance by providing an as operator. P96

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

   - One of the biggest improve- ments is that the generic collection classes allow you to work with collections of value types without requiring that items in the collection be boxed/unboxed. 
   - This in itself greatly im- proves performance because far fewer objects will be created on the managed heap, there- by reducing the number of garbage collections required by your application.
   - You will get compile-time type safety, and your source code will be cleaner due to fewer casts. 

9. If the `this` and obj arguments refer to the same object, return true. This step can improve performance when comparing objects with many fields. P139

10. Compilers tend to use the call instruction when calling methods defined by a value type because value types are sealed. This implies that there can be no `polymorphism` even for their `virtual` methods, which causes the performance of the call to be faster.

11. Many compilers will never emit code to call a value type’s default constructor automatically, even if the value type offers a parameterless constructor. P186

12. To improve performance and also to avoid considering an extension method that you may not want, the C# compiler requires that you “import” extension meth- ods. P201

13. 

## Hurt perfomance

1. The size of instances of your type is also a condition to take into account because by default, arguments are passed by value, which causes the fields in value type instances to be copied, hurting performance. P121 

   You should declare a type as a value type if one of the following statements is true:

   ​	■  Instances of the type are small (approximately 16 bytes or less).

   ​	■  Instances of the type are large (greater than 16 bytes) and are not passed as method param- eters 		or returned from methods.

2. Boxing and unboxing/copy operations hurt your application’s performance in terms of both speed and memory. P126 

3. At run time, the Microsoft.CSharp.dll assembly will have to load into the AppDomain, which hurts your application’s performance and increases memory consumption. P148

4. Boxing puts more pressure on the heap, forcing more frequent garbage collections and hurting performance. P166 

5. The CLR would have to verify at each write that the write was not occurring to a constant object, and this would hurt performance significantly. P225

6. 

# Recommend

1. If you have multiple types that can share a single version number and security settings, it is recommended that you place all of the types in a single file rather than spread the types out over separate files, let alone separate assemblies.  p46
2. When designing a type, you should try to minimize the number of virtual methods you define.
   - First, Calling a virtual method is slower than calling a nonvirtual method. 
   - Second, virtual methods cannot be inlined by the JIT compiler, which further hurts performance. 
   - Third, virtual methods make versioning of components more brittle.
   - Fourth, when defining a base type, it is common to offer a set of convenience overloaded methods. 
3. When normalizing strings, it is highly recommended that you use ToUpper­ Invariant instead of ToLowerInvariant because Microsoft has optimized the code for performing uppercase comparisons. P325
4. 

# Rules and Guidelines

## Define my own classes

- I will simulate creating a closed class by using the above technique of sealing the virtual methods that my class inherits.
-  I always define my data fields as private and I never waver on this. 
- I always define my methods, properties, and events as private and nonvir- tual. 



# Efficient

# Cannot

# Difference

## Value types and reference types

- Value type objects have two representations: an unboxed form and a boxed form,Reference types are always in a boxed form.
- Value types are derived from System.ValueType.
- You can’t define a new value type or a new reference type by using a value type as a base class, you shouldn’t introduce any new virtual methods into a value type. No methods can be abstract, and all methods are implicitly sealed (can’t be overridden).
- When you assign a value type variable to another value type variable, a field-by-field copy is made. When you assign a reference type variable to another reference type variable, only the memory address is copied.
- Two or more reference type variables can refer to a single object in the heap, allowing operations on one variable to affect the object referenced by the other variable. On the other hand, value type variables are distinct objects, and it’s not possible for operations on one value type variable to affect another.
- Because unboxed value types aren’t allocated on the heap, the storage allocated for them is freed as soon as the method that defines an instance of the type is no longer active as op- posed to waiting for a garbage collection.

## Sealed class vs unsealed class

- **Versioning**

- **Performance**

- **Security and predictability**

  

# Avoid



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



# Funny