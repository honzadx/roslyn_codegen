using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerators;

[Generator]
public class FastEnumSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is EnumDeclarationSyntax,
                transform: static (ctx, _) => Transform(ctx))
            .Where(s => s != null);
        context.RegisterSourceOutput(provider, GenerateCode);
    }

    private static EnumDeclarationSyntax Transform(GeneratorSyntaxContext context)
    {
        var syntax = (EnumDeclarationSyntax)context.Node;
        return syntax.HasAttribute("System.FlagsAttribute", context) ? syntax : null;
    }

    private static void GenerateCode(SourceProductionContext context, EnumDeclarationSyntax syntax)
    {
        using MemoryStream sourceStream = new();
        using StreamWriter sourceStreamWriter = new(sourceStream, Encoding.UTF8);
        using IndentedTextWriter codeWriter = new(sourceStreamWriter);
        
        codeWriter.WriteLine("//<auto-generated/>");
        codeWriter.WriteLine("using System.Runtime.CompilerServices;\n");

        var namespaceName = syntax.GetNamespaceName();
        namespaceName = string.IsNullOrEmpty(namespaceName) ? "Generated" : $"{namespaceName}.Generated";
        codeWriter.StartNamespaceScope(namespaceName);

        using (codeWriter.Scope(prefix: $"public static partial class {syntax.Identifier.Text}Extensions"))
        {
            using (codeWriter.Scope(
                prefix: $"internal static readonly {syntax.Identifier.Text}[] values = new[]", 
                inlinePostix: ";\n"))
            {
                foreach (var member in syntax.Members)
                {
                    codeWriter.Write(syntax.Identifier.Text);
                    codeWriter.Write('.');
                    codeWriter.Write(member.Identifier.Text);
                    codeWriter.WriteLine(',');
                }
            }
            
            codeWriter.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            using (codeWriter.Scope(
                prefix: $"public static {syntax.Identifier.Text}[] GetValues()", 
                postfix: string.Empty))
            {
                codeWriter.WriteLine("return values;");
            }
            
            codeWriter.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            using (codeWriter.Scope(
                prefix: $"public static bool HasFlagFast(this {syntax.Identifier.Text} self, {syntax.Identifier.Text} mask)",
                postfix: string.Empty))
            {
                codeWriter.WriteLine("return (self & mask) > 0;");
            }
            
            codeWriter.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            using (codeWriter.Scope(
                prefix: $"public static bool HasFlagFast(this {syntax.Identifier.Text} self, int mask)"))
            {
                codeWriter.WriteLine("return ((int)self & mask) > 0;");
            }
        }
        
        codeWriter.EndNamespaceScope(namespaceName);
        codeWriter.Flush();
        context.AddSource($"{syntax.Identifier.Text}Extensions.g.cs", SourceText.From(sourceStream, Encoding.UTF8, canBeEmbedded: true));
    }
}