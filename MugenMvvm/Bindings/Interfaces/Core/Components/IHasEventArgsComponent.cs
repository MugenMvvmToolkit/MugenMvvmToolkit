using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Bindings.Interfaces.Core.Components
{
    public interface IHasEventArgsComponent : IComponent<IBinding>
    {
        object? EventArgs { get; }
    }
}