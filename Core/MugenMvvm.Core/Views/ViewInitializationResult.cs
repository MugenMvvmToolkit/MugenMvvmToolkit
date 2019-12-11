using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Views
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ViewInitializationResult
    {
        #region Fields

        public readonly IReadOnlyMetadataContext Metadata;
        public readonly IView View;
        public readonly IViewModelBase ViewModel;

        #endregion

        #region Constructors

        public ViewInitializationResult(IView view, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(viewModel, nameof(viewModel));
            View = view;
            ViewModel = viewModel;
            Metadata = metadata.DefaultIfNull();
        }

        #endregion

        #region Properties

        public bool IsEmpty => View == null;

        #endregion
    }
}