using System;
using System.CodeDom.Compiler;

namespace SourceGenerators;

public class IndentedTextScope : IDisposable
{
    private IndentedTextWriter _writer;
    private readonly string _postfix;
    
    public IndentedTextScope(IndentedTextWriter writer, string prefix = null, string postfix = null)
    {
        _writer = writer;
        _postfix = postfix;
        
        if (prefix != null)
        {
            _writer.WriteLine(prefix);
        }
        _writer.StartScope();
    }

    ~IndentedTextScope()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_writer != null)
        {
            _writer.EndScope();
            if (_postfix != null)
            {
                _writer.WriteLine(_postfix);
            }
        }
        
        _writer = null;
        GC.SuppressFinalize(this);
    }
}