using MugenMvvm.Bindings.Enums;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Expressions
{
    public interface IUnaryExpressionNode : IExpressionNode
    {
        UnaryTokenType Token { get; }

        IExpressionNode Operand { get; }
    }
}