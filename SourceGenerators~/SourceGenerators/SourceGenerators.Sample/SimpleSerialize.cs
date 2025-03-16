using System;

[AttributeUsage(AttributeTargets.Struct)]
public class SimpleSerializeAttribute : System.Attribute;

[SimpleSerialize]
public partial struct SerializedStruct
{
    public int var0;
    public bool var1;
    public float var2;
}