using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Members;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IIndexExpressionNode : IHasTargetExpressionNode
    {
        IBindingMethodInfo? Indexer { get; }

        IReadOnlyList<IExpressionNode> Arguments { get; }
    }
}