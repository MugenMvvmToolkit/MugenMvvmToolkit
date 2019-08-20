using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing.Components
{
    public interface IBindingParserContextProviderComponent : IComponent<IBindingParser>
    {
        IBindingParserContext? TryGetBindingParserContext(object expression, IReadOnlyMetadataContext? metadata);
    }
}