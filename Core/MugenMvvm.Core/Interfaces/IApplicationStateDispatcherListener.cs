using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces
{
    public interface IApplicationStateDispatcherListener : IListener
    {
        void OnStateChanged(IApplicationStateDispatcher dispatcher, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext metadata);
    }
}