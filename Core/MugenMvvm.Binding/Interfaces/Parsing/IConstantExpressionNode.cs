using System;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IConstantExpressionNode : IExpressionNode
    {
        Type Type { get; }

        object? Value { get; }
    }
}