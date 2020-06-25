using MugenMvvm.Binding.Enums;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IUnaryExpressionNode : IExpressionNode
    {
        UnaryTokenType Token { get; }

        IExpressionNode Operand { get; }
    }
}