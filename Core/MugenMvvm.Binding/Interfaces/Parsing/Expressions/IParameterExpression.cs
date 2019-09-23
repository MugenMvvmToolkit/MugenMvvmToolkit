using System;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IParameterExpression : IExpressionNode
    {
        string Name { get; }

        Type? Type { get; }
    }
}