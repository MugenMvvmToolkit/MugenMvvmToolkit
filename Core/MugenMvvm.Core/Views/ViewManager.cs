using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views
{
    public sealed class ViewManager : ComponentOwnerBase<IViewManager>, IViewManager
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ViewManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IViewInfo> GetViews(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return GetComponents<IViewInfoProviderComponent>(metadata).TryGetViews(viewModel, metadata) ?? Default.EmptyArray<IViewInfo>();
        }

        public IReadOnlyList<IViewInitializer> GetInitializersByView(object view, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(view, nameof(view));
            return GetComponents<IViewInitializerProviderComponent>(metadata).TryGetInitializersByView(view, metadata) ?? Default.EmptyArray<IViewInitializer>();
        }

        public IReadOnlyList<IViewInitializer> GetInitializersByViewModel(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return GetComponents<IViewInitializerProviderComponent>(metadata).TryGetInitializersByViewModel(viewModel, metadata) ?? Default.EmptyArray<IViewInitializer>();
        }

        #endregion
    }
}