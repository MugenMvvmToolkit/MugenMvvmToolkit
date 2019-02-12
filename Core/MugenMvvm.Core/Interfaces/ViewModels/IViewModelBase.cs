using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.ViewModels
{
    public interface IViewModelBase : INotifyPropertyChangedEx, IDisposable, IHasMetadata<IObservableMetadataContext>
    {
    }
}