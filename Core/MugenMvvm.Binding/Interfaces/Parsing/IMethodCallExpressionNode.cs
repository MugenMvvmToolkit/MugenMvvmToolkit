using System.Collections.Generic;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IMethodCallExpressionNode : IExpressionNode
    {
        string Method { get; }

        IReadOnlyList<string> TypeArgs { get; }

        IExpressionNode? Target { get; }

        IReadOnlyList<IExpressionNode> Arguments { get; }
    }
}