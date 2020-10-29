namespace MugenMvvm.Bindings.Interfaces.Parsing.Expressions
{
    public interface IConditionExpressionNode : IExpressionNode
    {
        IExpressionNode Condition { get; }

        IExpressionNode IfTrue { get; }

        IExpressionNode IfFalse { get; }
    }
}