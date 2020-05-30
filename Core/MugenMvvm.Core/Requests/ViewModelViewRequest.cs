using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Requests
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ViewModelViewRequest
    {
        #region Fields

        public readonly object? View;
        public readonly IViewModelBase? ViewModel;

        #endregion

        #region Constructors

        public ViewModelViewRequest(IViewModelBase? viewModel, object? view)
        {
            ViewModel = viewModel;
            View = view;
        }

        #endregion
    }
}