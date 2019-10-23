namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IHasTargetExpressionNode : IExpressionNode
    {
        IExpressionNode? Target { get; }

        IHasTargetExpressionNode UpdateTarget(IExpressionNode? target);
    }
}