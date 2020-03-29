using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Binding.Interfaces.Parsing.Components
{
    public interface IExpressionConverterComponent<TExpression> : IComponent<IExpressionParser> where TExpression : class
    {
        IExpressionNode? TryConvert(IExpressionConverterContext<TExpression> context, TExpression expression);
    }
}