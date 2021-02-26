using System;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Expressions
{
    public interface ITypeAccessExpressionNode : IExpressionNode
    {
        Type Type { get; }
    }
}