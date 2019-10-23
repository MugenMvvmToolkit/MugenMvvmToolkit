using System.Collections.Generic;
using System.Reflection;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IIndexExpressionNode : IHasTargetExpressionNode
    {
        PropertyInfo? Indexer { get; }

        IReadOnlyList<IExpressionNode> Arguments { get; }
    }
}