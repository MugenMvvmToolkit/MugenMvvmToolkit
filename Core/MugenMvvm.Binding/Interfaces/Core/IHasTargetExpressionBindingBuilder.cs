using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IHasTargetExpressionBindingBuilder : IBindingBuilder
    {
        IExpressionNode TargetExpression { get; }
    }
}