using System.Collections.Generic;
using System.Reflection;

namespace MugenMvvm.Binding.Interfaces.Parsing.Nodes
{
    public interface IMethodCallExpressionNode : IExpressionNode
    {
        MethodInfo? Method { get; }

        string MethodName { get; }

        IReadOnlyList<string> TypeArgs { get; }

        IExpressionNode? Target { get; }

        IReadOnlyList<IExpressionNode> Arguments { get; }
    }
}