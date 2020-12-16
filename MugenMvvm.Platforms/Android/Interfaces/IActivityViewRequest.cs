using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Android.Interfaces
{
    public interface IActivityViewRequest
    {
        object? View { get; set; }

        IViewModelBase? ViewModel { get; set; }

        IViewMapping Mapping { get; }

        bool IsTargetActivity(object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata);

        void StartActivity();
    }
}