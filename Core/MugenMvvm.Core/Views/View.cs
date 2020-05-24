using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Metadata;

namespace MugenMvvm.Views
{
    public sealed class View : MetadataOwnerBase, IView
    {
        #region Fields

        private readonly IComponentCollectionProvider? _componentCollectionProvider;
        private readonly object _view;
        private IComponentCollection? _components;

        #endregion

        #region Constructors

        public View(IViewModelViewMapping mapping, object view, IComponentCollectionProvider? componentCollectionProvider = null, IReadOnlyMetadataContext? metadata = null,
            IMetadataContextProvider? metadataContextProvider = null)
            : base(metadata, metadataContextProvider)
        {
            Should.NotBeNull(mapping, nameof(mapping));
            Should.NotBeNull(view, nameof(view));
            Mapping = mapping;
            _view = view;
            _componentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        public IViewModelViewMapping Mapping { get; }

        object IView.View => _view;

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

        #endregion
    }
}