using System;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Expressions
{
    public interface IConstantExpressionNode : IExpressionNode
    {
        Type Type { get; }

        object? Value { get; }
    }
}