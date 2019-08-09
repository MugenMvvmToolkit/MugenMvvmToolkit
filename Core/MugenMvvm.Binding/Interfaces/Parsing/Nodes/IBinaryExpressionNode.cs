using MugenMvvm.Binding.Enums;

namespace MugenMvvm.Binding.Interfaces.Parsing.Nodes
{
    public interface IBinaryExpressionNode : IExpressionNode
    {
        IExpressionNode Left { get; }

        IExpressionNode Right { get; }

        BinaryTokenType Token { get; }
    }
}