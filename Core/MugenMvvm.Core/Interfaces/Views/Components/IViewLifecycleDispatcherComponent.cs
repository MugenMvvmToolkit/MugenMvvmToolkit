using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewLifecycleDispatcherComponent : IComponent<IViewManager>
    {
        void OnLifecycleChanged<TState>(IView view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata);
    }
}