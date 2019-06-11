using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views.Infrastructure;

namespace MugenMvvm.Infrastructure.Views
{
    public class ViewDataContextProvider : IViewDataContextProvider
    {
        #region Fields

        private IComponentCollection<IChildViewDataContextProvider> _providers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewDataContextProvider(IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            ComponentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        protected IComponentCollectionProvider ComponentCollectionProvider { get; }

        public IComponentCollection<IChildViewDataContextProvider> Providers
        {
            get
            {
                if (_providers == null)
                    ComponentCollectionProvider.LazyInitialize(ref _providers, this);
                return _providers;
            }
        }

        #endregion

        #region Implementation of interfaces

        public object GetDataContext(object view, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetDataContextInternal(view, metadata);
        }

        public bool SetDataContext(object view, object? dataContext, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(metadata, nameof(metadata));
            return SetDataContextInternal(view, dataContext, metadata);
        }

        #endregion

        #region Methods

        protected virtual object? GetDataContextInternal(object view, IReadOnlyMetadataContext metadata)
        {
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i].TryGetDataContext(this, view, metadata, out var context))
                    return context;
            }

            return null;
        }

        protected virtual bool SetDataContextInternal(object view, object? dataContext, IReadOnlyMetadataContext metadata)
        {
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i].TrySetDataContext(this, view, dataContext, metadata))
                    return true;
            }

            return false;
        }

        #endregion
    }
}