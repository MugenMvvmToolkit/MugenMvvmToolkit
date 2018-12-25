using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.ViewModels
{
    public interface IViewModel : ISuspendNotifications, IDisposableObject, IHasMemento
    {
        IMessenger InternalMessenger { get; }

        IBusyIndicatorProvider BusyIndicatorProvider { get; }

        IObservableMetadataContext Metadata { get; }
    }
}