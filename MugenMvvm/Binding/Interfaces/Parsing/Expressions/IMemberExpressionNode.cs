namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IMemberExpressionNode : IHasTargetExpressionNode<IMemberExpressionNode>
    {
        string Member { get; }
    }
}