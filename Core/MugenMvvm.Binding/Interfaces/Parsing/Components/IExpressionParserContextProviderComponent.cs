using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing.Components
{
    public interface IExpressionParserContextProviderComponent : IComponent<IExpressionParser>
    {
        IExpressionParserContext? TryGetBindingParserContext(object expression, IReadOnlyMetadataContext? metadata);
    }
}