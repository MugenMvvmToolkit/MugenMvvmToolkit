using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IHasTargetExpressionBindingExpression : IBindingExpression
    {
        IExpressionNode TargetExpression { get; }
    }
}