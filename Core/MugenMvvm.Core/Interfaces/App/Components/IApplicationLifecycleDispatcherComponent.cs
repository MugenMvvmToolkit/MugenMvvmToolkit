using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.App.Components
{
    public interface IApplicationLifecycleDispatcherComponent : IComponent<IMugenApplication>
    {
        void OnLifecycleChanged<TState>(IMugenApplication application, ApplicationLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata);
    }
}