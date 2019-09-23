using System.Collections.Generic;
using System.Reflection;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IIndexExpressionNode : IExpressionNode
    {
        PropertyInfo? Indexer { get; }

        IExpressionNode? Target { get; }

        IReadOnlyList<IExpressionNode> Arguments { get; }
    }
}