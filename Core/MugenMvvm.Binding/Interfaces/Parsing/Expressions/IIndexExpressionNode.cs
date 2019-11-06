using MugenMvvm.Binding.Interfaces.Members;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IIndexExpressionNode : IHasTargetExpressionNode<IIndexExpressionNode>, IHasArgumentsExpressionNode<IIndexExpressionNode>
    {
        IMethodInfo? Indexer { get; }
    }
}