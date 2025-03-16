namespace SourceGenerators;

public static class StringExtensions
{
    public static string CapitalizeFirst(this string self)
    {
        if (string.IsNullOrEmpty(self))
        {
            return self;
        }
        
        return char.ToUpper(self[0]) + self.Substring(1);
    }
    
    public static string DecapitalizeFirst(this string self)
    {
        if (string.IsNullOrEmpty(self))
        {
            return self;
        }
        
        return char.ToLower(self[0]) + self.Substring(1);
    }
}