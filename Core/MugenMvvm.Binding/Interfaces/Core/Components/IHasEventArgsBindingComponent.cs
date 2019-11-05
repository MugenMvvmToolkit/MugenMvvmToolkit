using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IHasEventArgsBindingComponent : IComponent<IBinding>
    {
        object? EventArgs { get; }
    }
}