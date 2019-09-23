using System.Collections.Generic;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface ILambdaExpressionNode : IExpressionNode
    {
        IReadOnlyList<IParameterExpression> Parameters { get; }

        IExpressionNode Body { get; }
    }
}