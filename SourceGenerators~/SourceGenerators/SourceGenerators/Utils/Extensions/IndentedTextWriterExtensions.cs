using System.CodeDom.Compiler;

namespace SourceGenerators;

public static class IndentedTextWriterExtensions
{
    public static IndentedTextScope Scope(this IndentedTextWriter self, string prefix = null, string postfix = null)
    {
        return new IndentedTextScope(self, prefix, postfix);
    }
    
    public static void StartScope(this IndentedTextWriter self)
    {
        self.WriteLine("{");
        self.Indent++;
    }
    
    public static void EndScope(this IndentedTextWriter self)
    {
        self.Indent--;
        self.WriteLine("}");
    }
    
    public static void StartNamespaceScope(this IndentedTextWriter self, string namespaceName)
    {
        if (string.IsNullOrEmpty(namespaceName))
        {
            return;
        }
        self.Write("namespace ");
        self.WriteLine(namespaceName);
        self.WriteLine('{');
        self.Indent++;
    }
    
    public static void EndNamespaceScope(this IndentedTextWriter self, string namespaceName)
    {
        if (string.IsNullOrEmpty(namespaceName))
        {
            return;
        }
        self.Indent--;
        self.WriteLine('}');
    }
}