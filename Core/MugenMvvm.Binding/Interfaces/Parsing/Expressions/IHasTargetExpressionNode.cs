namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IHasTargetExpressionNode<out TExpression> : IExpressionNode where TExpression : class, IExpressionNode
    {
        IExpressionNode? Target { get; }

        TExpression UpdateTarget(IExpressionNode? target);
    }
}