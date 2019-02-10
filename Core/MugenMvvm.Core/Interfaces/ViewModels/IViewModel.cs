using System;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.ViewModels
{
    public interface IViewModelBase : INotifyPropertyChangedEx, IHasMetadata<IObservableMetadataContext>
    {

    }

    public interface IHasService<out TService> where TService : class
    {
        TService Service { get; }
    }

    public interface IViewModel : INotifyPropertyChangedEx, IDisposable, IHasMemento, IHasMetadata<IObservableMetadataContext>, IEventPublisher
    {
        IMessenger InternalMessenger { get; }

        IBusyIndicatorProvider BusyIndicatorProvider { get; }
    }
}