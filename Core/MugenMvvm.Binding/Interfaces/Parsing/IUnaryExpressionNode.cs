using MugenMvvm.Binding.Enums;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IUnaryExpressionNode : IExpressionNode
    {
        UnaryTokenType Token { get; }

        IExpressionNode Operand { get; }
    }
}