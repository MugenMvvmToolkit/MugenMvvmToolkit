using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources.Components
{
    public interface IBindingValueConverterResolverComponent : IComponent<IBindingResourceResolver>
    {
        IBindingValueConverter? TryGetConverter(string name, IReadOnlyMetadataContext? metadata);
    }
}