using System.Collections.Generic;
using MugenMvvm.Collections;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Expressions
{
    public interface IMethodCallExpressionNode : IHasTargetExpressionNode<IMethodCallExpressionNode>, IHasArgumentsExpressionNode<IMethodCallExpressionNode>
    {
        string Method { get; }

        ItemOrIReadOnlyList<string> TypeArgs { get; }
    }
}