using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IBindingComponentProvider
    {
        IComponent<IBinding>? GetComponent(IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata);
    }
}