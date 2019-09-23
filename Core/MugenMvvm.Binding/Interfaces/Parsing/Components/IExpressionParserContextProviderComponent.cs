using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing.Components
{
    public interface IExpressionParserContextProviderComponent : IComponent<IExpressionParser>
    {
    }

    public interface IExpressionParserContextProviderComponent<TExpression> : IExpressionParserContextProviderComponent
    {
        IExpressionParserContext? TryGetParserContext(in TExpression expression, IReadOnlyMetadataContext? metadata);
    }
}