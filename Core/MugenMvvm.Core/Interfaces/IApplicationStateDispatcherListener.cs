using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces
{
    public interface IApplicationStateDispatcherListener
    {
        void OnStateChanged(IApplicationStateDispatcher dispatcher, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext metadata);
    }
}