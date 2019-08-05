using System.Collections.Generic;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IIndexExpressionNode : IExpressionNode
    {
        IExpressionNode Target { get; }

        IReadOnlyList<IExpressionNode> Arguments { get; }
    }
}