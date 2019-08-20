using System;

namespace MugenMvvm.Binding.Interfaces.Parsing.Nodes
{
    public interface IParameterExpression : IExpressionNode
    {
        string Name { get; }

        Type? Type { get; }
    }
}