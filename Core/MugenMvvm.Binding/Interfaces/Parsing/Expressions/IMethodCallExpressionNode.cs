using System.Collections.Generic;
using System.Reflection;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IMethodCallExpressionNode : IHasTargetExpressionNode
    {
        MethodInfo? Method { get; }

        string MethodName { get; }

        IReadOnlyList<string> TypeArgs { get; }

        IReadOnlyList<IExpressionNode> Arguments { get; }
    }
}