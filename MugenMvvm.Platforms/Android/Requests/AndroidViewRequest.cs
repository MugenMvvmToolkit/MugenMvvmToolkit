using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Android.Requests
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct AndroidViewRequest
    {
        #region Fields

        public readonly IViewModelBase ViewModel;
        public readonly object Container;
        public readonly int ResourceId;

        #endregion

        #region Constructors

        public AndroidViewRequest(IViewModelBase viewModel, object container, int resourceId)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(container, nameof(container));
            Container = container;
            ViewModel = viewModel;
            ResourceId = resourceId;
        }

        #endregion
    }
}