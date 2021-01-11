using MugenMvvm.Collections;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Expressions
{
    public interface IHasArgumentsExpressionNode<out TExpression> : IExpressionNode where TExpression : class, IExpressionNode
    {
        ItemOrIReadOnlyList<IExpressionNode> Arguments { get; }

        TExpression UpdateArguments(ItemOrIReadOnlyList<IExpressionNode> arguments);
    }
}