using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Metadata;

namespace MugenMvvm.Views
{
    public sealed class View : MetadataOwnerBase, IView
    {
        #region Fields

        private readonly IComponentCollectionProvider? _componentCollectionProvider;
        private IComponentCollection? _components;

        #endregion

        #region Constructors

        public View(IViewModelViewMapping mapping, object view, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null,
            IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(metadata, metadataContextProvider)
        {
            Should.NotBeNull(mapping, nameof(mapping));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Mapping = mapping;
            ViewModel = viewModel;
            Target = view;
            _componentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        public bool HasComponents => _components != null && _components.Count != 0;

        public IComponentCollection Components
        {
            get
            {
                if (_components == null)
                    _componentCollectionProvider.DefaultIfNull().LazyInitialize(ref _components, this);
                return _components;
            }
        }

        public IViewModelViewMapping Mapping { get; }

        public IViewModelBase ViewModel { get; }

        public object Target { get; }

        #endregion
    }
}