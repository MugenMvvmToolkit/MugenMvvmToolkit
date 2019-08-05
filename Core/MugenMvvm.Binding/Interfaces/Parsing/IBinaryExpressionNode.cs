using MugenMvvm.Binding.Enums;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IBinaryExpressionNode : IExpressionNode
    {
        IExpressionNode Left { get; }

        IExpressionNode Right { get; }

        BinaryTokenType Token { get; }
    }
}