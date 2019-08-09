using MugenMvvm.Binding.Enums;

namespace MugenMvvm.Binding.Interfaces.Parsing.Nodes
{
    public interface IUnaryExpressionNode : IExpressionNode
    {
        UnaryTokenType Token { get; }

        IExpressionNode Operand { get; }
    }
}