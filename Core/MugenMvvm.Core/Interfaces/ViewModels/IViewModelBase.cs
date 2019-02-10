using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.ViewModels
{
    public interface IHasService<out TService> where TService : class
    {
        TService Service { get; }
    }

    public interface IViewModelBase : INotifyPropertyChangedEx, IDisposable, IHasMetadata<IObservableMetadataContext>
    {
    }
}