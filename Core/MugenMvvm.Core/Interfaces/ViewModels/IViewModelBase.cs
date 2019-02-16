using System.ComponentModel;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.ViewModels
{
    public interface IViewModelBase : INotifyPropertyChanged, IHasMetadata<IObservableMetadataContext>
    {
    }
}