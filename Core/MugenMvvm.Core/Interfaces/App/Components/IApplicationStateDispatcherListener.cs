using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.App.Components
{
    public interface IApplicationStateDispatcherListener : IComponent<IApplicationStateDispatcher>
    {
        void OnStateChanged(IApplicationStateDispatcher dispatcher, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext metadata);
    }
}