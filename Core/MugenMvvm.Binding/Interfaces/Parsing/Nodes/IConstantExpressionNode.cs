using System;

namespace MugenMvvm.Binding.Interfaces.Parsing.Nodes
{
    public interface IConstantExpressionNode : IExpressionNode
    {
        Type Type { get; }

        object? Value { get; }
    }
}