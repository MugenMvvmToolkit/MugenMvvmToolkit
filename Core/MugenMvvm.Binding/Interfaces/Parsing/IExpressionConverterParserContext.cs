using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IExpressionConverterParserContext<in TExpression> : IParserContext where TExpression : class
    {
        IExpressionNode? TryGetExpression(TExpression expression);

        void SetExpression(TExpression expression, IExpressionNode value);

        void ClearExpression(TExpression expression);

        IExpressionNode Convert(TExpression expression);
    }
}