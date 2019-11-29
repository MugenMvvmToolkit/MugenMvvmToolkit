using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IBindingComponentBuilder
    {
        bool IsEmpty { get; }

        string Name { get; }

        IComponent<IBinding>? GetComponent(IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata);
    }
}