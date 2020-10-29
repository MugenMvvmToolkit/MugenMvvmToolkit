using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Bindings.Interfaces.Core.Components.Binding
{
    public interface IHasEventArgsBindingComponent : IComponent<IBinding>
    {
        object? EventArgs { get; }
    }
}