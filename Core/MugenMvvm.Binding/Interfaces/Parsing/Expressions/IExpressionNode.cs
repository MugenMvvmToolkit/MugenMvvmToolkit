using MugenMvvm.Binding.Enums;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IExpressionNode
    {
        ExpressionNodeType NodeType { get; }

        IExpressionNode Accept(IExpressionVisitor visitor);
    }
}