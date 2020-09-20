using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Android.Interfaces
{
    public interface IActivityViewRequest
    {
        object? View { get; set; }

        IViewModelBase? ViewModel { get; set; }

        IViewMapping Mapping { get; }

        void StartActivity();
    }
}