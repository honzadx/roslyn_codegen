using System.CodeDom.Compiler;

namespace SourceGenerators;

public static class IndentedTextWriterExtensions
{
    public static IndentedTextScope Scope(this IndentedTextWriter self, string prefix = null, string inlinePostix = null, string postfix = null)
    {
        return new IndentedTextScope(self, prefix, inlinePostix, postfix);
    }
    
    public static void StartScope(this IndentedTextWriter self)
    {
        self.WriteLine("{");
        self.Indent++;
    }
    
    public static void EndScope(this IndentedTextWriter self, string inlinePostfix = null)
    {
        string endScopeText = "}";
        if (inlinePostfix != null)
        {
            endScopeText += inlinePostfix;
        }
        self.Indent--;
        self.WriteLine(endScopeText);
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