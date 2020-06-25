using MugenMvvm.Binding.Enums;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IBinaryExpressionNode : IExpressionNode
    {
        IExpressionNode Left { get; }

        IExpressionNode Right { get; }

        BinaryTokenType Token { get; }
    }
}