namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IConditionExpressionNode : IExpressionNode
    {
        IExpressionNode Condition { get; }

        IExpressionNode IfTrue { get; }

        IExpressionNode IfFalse { get; }
    }
}