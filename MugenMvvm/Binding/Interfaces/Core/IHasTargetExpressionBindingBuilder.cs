using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Bindings.Interfaces.Core
{
    public interface IHasTargetExpressionBindingBuilder : IBindingBuilder
    {
        IExpressionNode TargetExpression { get; }
    }
}