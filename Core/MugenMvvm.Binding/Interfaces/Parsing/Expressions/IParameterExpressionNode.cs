using System;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IParameterExpressionNode : IExpressionNode
    {
        int Index { get; }

        string Name { get; }

        Type? Type { get; }
    }
}