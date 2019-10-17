using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IBindingComponentBuilder
    {
        string Name { get; }

        IComponent<IBinding> GetComponent(object target, object? source, IReadOnlyMetadataContext? metadata);
    }
}