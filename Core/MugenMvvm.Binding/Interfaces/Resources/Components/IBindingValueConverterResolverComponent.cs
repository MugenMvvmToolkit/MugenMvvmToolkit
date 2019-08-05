using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources.Components
{
    public interface IBindingValueConverterResolverComponent : IComponent<IBindingManager>
    {
        IBindingValueConverter? TryGetConverter(string name, IReadOnlyMetadataContext? metadata);
    }
}