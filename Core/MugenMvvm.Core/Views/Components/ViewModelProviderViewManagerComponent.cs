using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewModelProviderViewManagerComponent : AttachableComponentBase<IViewManager>, IViewModelProviderViewManagerComponent, IHasPriority
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private readonly IViewModelDispatcher? _viewModelDispatcher;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelProviderViewManagerComponent(IViewModelDispatcher? viewModelDispatcher = null, IMetadataContextProvider? metadataContextProvider = null)
        {
            _viewModelDispatcher = viewModelDispatcher;
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public IViewModelBase? TryGetViewModelForView(IViewInitializer initializer, object view, IMetadataContext metadata)
        {
            Should.NotBeNull(initializer, nameof(initializer));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(metadata, nameof(metadata));
            var metadataContext = _metadataContextProvider.ServiceIfNull().GetMetadataContext(this, metadata);
            metadataContext.Set(ViewModelMetadata.Type, initializer.ViewModelType);
            var viewModel = _viewModelDispatcher.ServiceIfNull().TryGetViewModel(metadataContext);
            if (viewModel != null)
            {
                var components = Owner.GetComponents();
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IViewManagerListener)?.OnViewModelCreated(Owner, viewModel, view, metadata);
            }
            return viewModel;
        }

        #endregion
    }
}