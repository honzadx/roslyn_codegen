using System;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public class ClonableAttribute : Attribute;

[Clonable]
public partial struct ClonableStruct
{
    public string name;
    public int num1;
    public double num2;
    public NotClonableClass reference;
}

namespace A.B.C
{
    using System.Runtime.CompilerServices;
    
    [Clonable]
    public partial class ClonableClass
    {
        public string? name;
        public int num1;
        public double num2;
        public NotClonableClass? reference;
    }    
}

public class NotClonableClass
{
    
}