using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerators;

[Generator]
public class SimpleSerializeSourceGenerator : IIncrementalGenerator
{
    private const string SIMPLE_SERIALIZE_ATTRIBUTE_NAME = "SimpleSerializeAttribute";

    private static readonly string[] serializedTypes = { "bool", "int", "float" };
    private static readonly Dictionary<string, char> typeToToken = new ()
    {
        { "bool", 'b' },
        { "int", 'i' },
        { "float", 'f' },
    };
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is StructDeclarationSyntax,
                transform: static (ctx, _) => Transform(ctx))
            .Where(s => s != null);
        
        context.RegisterSourceOutput(provider, GenerateCode);
    }

    private static StructDeclarationSyntax Transform(GeneratorSyntaxContext context)
    {
        var syntax = (StructDeclarationSyntax)context.Node;

        if (!syntax.HasModifiers(["public", "partial"]))
        {
            return null;
        }
        if (!syntax.HasAttribute(SIMPLE_SERIALIZE_ATTRIBUTE_NAME, context))
        {
            return null;
        }
                    
        return syntax;
    }

    private static void GenerateCode(SourceProductionContext context, StructDeclarationSyntax syntax)
    {
        using MemoryStream sourceStream = new();
        using StreamWriter sourceStreamWriter = new(sourceStream);
        using IndentedTextWriter codeWriter = new(sourceStreamWriter);
        
        List<VariableDeclarationSyntax> variables = new();
        foreach (var member in syntax.Members)
        foreach (var node in member.ChildNodes())
        {
            if (node is not VariableDeclarationSyntax variable)
            {
                continue;
            }
            if (!IsTypeSupported(variable.Type) || variable.Variables.Count > 1)
            {
                continue;
            }
            variables.Add(variable);
        }

        codeWriter.WriteLine("//<auto-generated />");
        codeWriter.WriteLine("using System;");
        codeWriter.WriteLine("using System.IO;");
        codeWriter.WriteLine("using System.Diagnostics;");
        codeWriter.WriteLine("using System.Text;\n");

        var namespaceName = syntax.GetNamespaceName();
        codeWriter.StartNamespaceScope(namespaceName);

        using (codeWriter.Scope(prefix: $"partial struct {syntax.Identifier.Text}"))
        {
            using (codeWriter.Scope(prefix: "public string Serialize()", postfix: string.Empty))
            {
                codeWriter.WriteLine("StringBuilder builder = new();");
                foreach (var variable in variables)
                {
                    SerializeVariable(variable, codeWriter);
                }
                codeWriter.WriteLine("return builder.ToString();");
            }
            
            using (codeWriter.Scope(prefix: "public bool Deserialize(string data)"))
            {
                codeWriter.WriteLine("try");
                using (codeWriter.Scope())
                {
                    codeWriter.WriteLine("using (StringReader reader = new(data))");
                    using (codeWriter.Scope())
                    {
                        for (int i = 0; i < variables.Count; i++)
                        {
                            ValidateDeserialization(variables[i], i, codeWriter);
                            codeWriter.WriteLine();
                        }
                        
                        for (int i = 0; i < variables.Count; i++)
                        {
                            DeserializeVariable(variables[i], i, codeWriter);
                        }
                    }
                }
                codeWriter.WriteLine("catch (Exception e)");
                using (codeWriter.Scope())
                {
                    codeWriter.WriteLine("Console.Write(e);");
                    codeWriter.WriteLine("return false;");
                }
                codeWriter.WriteLine("return true;");
            }
        }
        
        codeWriter.EndNamespaceScope(namespaceName);
        
        codeWriter.Flush();
        context.AddSource($"{syntax.Identifier.Text}.g.cs", SourceText.From(sourceStream, Encoding.UTF8, canBeEmbedded: true));
    }

    private static void SerializeVariable(VariableDeclarationSyntax variable, IndentedTextWriter codeWriter)
    {
        codeWriter.Write("builder.Append($\"");
        codeWriter.Write(typeToToken[variable.Type.ToString()]);
        codeWriter.Write(":{");
        codeWriter.Write(variable.Variables[0].Identifier.Text);
        codeWriter.WriteLine("}\");");
    }

    private static void ValidateDeserialization(VariableDeclarationSyntax variable, int variableIndex, IndentedTextWriter codeWriter)
    {
        string typeString = variable.Type.ToString();
        string lineName = $"line{variableIndex}";
        string typeTokenName = $"typeToken{variableIndex}";
        string valueTokenName = $"valueToken{variableIndex}";
        codeWriter.WriteLine($"var {lineName} = reader.ReadLine();");
        codeWriter.WriteLine($"Debug.Assert({lineName} != null);");
        codeWriter.WriteLine($"var {typeTokenName} = {lineName}[0];");
        codeWriter.WriteLine($"var {valueTokenName} = {lineName}.Substring(2, {lineName}.Length - 2);");
        codeWriter.WriteLine($"Debug.Assert({typeTokenName} == '{typeToToken[typeString]}');");
    }

    private static void DeserializeVariable(VariableDeclarationSyntax variable, int variableIndex, IndentedTextWriter codeWriter)
    {
        string valueTokenName = $"valueToken{variableIndex}";
        string typeString = variable.Type.ToString();
        switch (typeString)
        {
            case "bool":
                codeWriter.WriteLine($"bool.TryParse({valueTokenName}, out {variable.Variables[0].Identifier.Text});");
                break;
            case "float":
                codeWriter.WriteLine($"float.TryParse({valueTokenName}, out {variable.Variables[0].Identifier.Text});");
                break;
            case "int":
                codeWriter.WriteLine($"int.TryParse({valueTokenName}, out {variable.Variables[0].Identifier.Text});");
                break;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsTypeSupported(TypeSyntax type)
    {
        return serializedTypes.Contains(type.ToString());
    }
}
