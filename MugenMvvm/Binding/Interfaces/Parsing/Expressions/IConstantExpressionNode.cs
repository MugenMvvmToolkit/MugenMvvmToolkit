using System;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IConstantExpressionNode : IExpressionNode
    {
        Type Type { get; }

        object? Value { get; }
    }
}