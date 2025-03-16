
using System;

[AttributeUsage(AttributeTargets.Field)]
public class GetAttribute : System.Attribute;

[AttributeUsage(AttributeTargets.Field)]
public class SetAttribute : System.Attribute;

namespace A.B.C
{
    using D.E.F;
    
    public partial class AccessibleClass
    {
        private int _index;
        [Get]
        private string _name;
        [Get, Set]
        private int _count;
        [Get, Set]
        private Date _date;
    }
}

public partial struct AccessibleStruct
{
    private int _index;
    [Get]
    private string _name;
    [Get, Set]
    private int _count;
    [Get, Set]
    private D.E.F.Date _date;
}

namespace D.E.F
{
    public struct Date
    {
        public int year;
        public int month;
        public int day;
    }
}