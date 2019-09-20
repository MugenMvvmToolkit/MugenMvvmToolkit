using System.ComponentModel;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels
{
    public interface IViewModelBase : INotifyPropertyChanged, IMetadataOwner<IMetadataContext>
    {
    }
}