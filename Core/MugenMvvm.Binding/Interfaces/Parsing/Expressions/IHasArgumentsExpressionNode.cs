using System.Collections.Generic;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IHasArgumentsExpressionNode<out TExpression> : IExpressionNode where TExpression : class, IExpressionNode
    {
        IReadOnlyList<IExpressionNode> Arguments { get; }

        TExpression UpdateArguments(IReadOnlyList<IExpressionNode> arguments);
    }
}