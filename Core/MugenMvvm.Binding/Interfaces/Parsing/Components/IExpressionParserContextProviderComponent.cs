using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing.Components
{
    public interface IExpressionParserContextProviderComponent : IComponent<IExpressionParser>
    {
        IExpressionParserContext? TryGetParserContext(object expression, IReadOnlyMetadataContext? metadata);
    }
}