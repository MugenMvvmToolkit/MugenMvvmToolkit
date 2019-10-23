using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Members;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IMethodCallExpressionNode : IHasTargetExpressionNode<IMethodCallExpressionNode>, IHasArgumentsExpressionNode<IMethodCallExpressionNode>
    {
        IBindingMethodInfo? Method { get; }

        string MethodName { get; }

        IReadOnlyList<string> TypeArgs { get; }
    }
}