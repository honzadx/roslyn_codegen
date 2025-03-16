using System;

namespace A.B.C
{
    [Flags]
    public enum ColorFlags
    {
        Red,
        Green,
        Blue,
        Alpha
    }

    // Test extension collisions 
    public static class ColorFlagsExtensions
    {
        public static bool IsRed(this ColorFlags self)
        {
            return (self & ColorFlags.Red) > 0;
        }
        
        public static bool IsBlue(this ColorFlags self)
        {
            return (self & ColorFlags.Red) > 0;
        }
        
        public static bool IsGreen(this ColorFlags self)
        {
            return (self & ColorFlags.Red) > 0;
        }
    } 
}
