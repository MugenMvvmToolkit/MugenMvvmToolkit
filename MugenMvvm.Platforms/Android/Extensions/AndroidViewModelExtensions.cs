using Java.Lang;
using MugenMvvm.Android.Requests;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Views;

namespace MugenMvvm.Android.Extensions
{
    public static partial class MugenAndroidExtensions
    {
        #region Methods

        public static IView GetOrCreateView(this IViewModelBase viewModel, Object container, int resourceId, IReadOnlyMetadataContext? metadata = null, IViewManager? viewManager = null) =>
            viewManager.DefaultIfNull().InitializeAsync(ViewMapping.Undefined, new AndroidViewRequest(viewModel, container, resourceId), default, metadata).Result;

        #endregion
    }
}