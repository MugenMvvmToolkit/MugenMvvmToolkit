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

        private readonly IComponentCollectionManager? _componentCollectionManager;
        private IComponentCollection? _components;

        #endregion

        #region Constructors

        public View(IViewMapping mapping, object view, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null,
            IComponentCollectionManager? componentCollectionManager = null)
            : base(metadata)
        {
            Should.NotBeNull(mapping, nameof(mapping));
            Should.BeOfType(view, mapping.ViewType, nameof(view));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Mapping = mapping;
            ViewModel = viewModel;
            Target = view;
            _componentCollectionManager = componentCollectionManager;
        }

        #endregion

        #region Properties

        public bool HasComponents => _components != null && _components.Count != 0;

        public IComponentCollection Components => _components ?? _componentCollectionManager.DefaultIfNull().EnsureInitialized(ref _components, this);

        public IViewMapping Mapping { get; }

        public IViewModelBase ViewModel { get; }

        public object Target { get; }

        #endregion
    }
}