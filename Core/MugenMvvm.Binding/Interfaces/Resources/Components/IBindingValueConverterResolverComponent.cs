using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources.Components
{
    public interface IBindingValueConverterResolverComponent : IComponent<IResourceResolver>
    {
        IBindingValueConverter? TryGetConverter<TRequest>(string name, in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}