using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
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
        private readonly IViewModelManager? _viewModelDispatcher;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelProviderViewManagerComponent(IViewModelManager? viewModelDispatcher = null, IMetadataContextProvider? metadataContextProvider = null)
        {
            _viewModelDispatcher = viewModelDispatcher;
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.ViewModelProvider;

        #endregion

        #region Implementation of interfaces

        public IViewModelBase? TryGetViewModelForView(IViewInitializer initializer, object view, IMetadataContext metadata)
        {
            Should.NotBeNull(initializer, nameof(initializer));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(metadata, nameof(metadata));
            var metadataContext = _metadataContextProvider
                .DefaultIfNull()
                .GetReadOnlyMetadataContext(this, MetadataContextValue.Create(ViewModelMetadata.Type, initializer.ViewModelType));
            return _viewModelDispatcher.DefaultIfNull().TryGetViewModel(metadataContext);
        }

        #endregion
    }
}