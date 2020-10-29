using MugenMvvm.Bindings.Enums;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Expressions
{
    public interface IBinaryExpressionNode : IExpressionNode
    {
        IExpressionNode Left { get; }

        IExpressionNode Right { get; }

        BinaryTokenType Token { get; }
    }
}