using System;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Observers
{
    public class BindingObserverProvider : IBindingObserverProvider
    {
        #region Fields

        private IComponentCollection<IChildBindingObserverProvider>? _providers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public BindingObserverProvider(IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            ComponentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        public IComponentCollection<IChildBindingObserverProvider> Providers
        {
            get
            {
                if (_providers == null)
                    ComponentCollectionProvider.LazyInitialize(ref _providers, this);
                return _providers;
            }
        }

        protected IComponentCollectionProvider ComponentCollectionProvider { get; }

        #endregion

        #region Implementation of interfaces

        public bool TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext metadata, out BindingMemberObserver observer)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(metadata, nameof(metadata));
            return TryGetMemberObserverInternal(type, member, metadata, out observer);
        }

        public IBindingPath GetBindingPath(object path, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(metadata, nameof(metadata));
            var p = path as IBindingPath ?? TryGetBindingPathInternal(path, metadata);
            if (p == null)
                ExceptionManager.ThrowNotSupported(nameof(path));
            return p;
        }

        public IBindingPathObserver GetBindingPathObserver(object source, object path, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(source, nameof(source));
            var bindingPath = GetBindingPath(path, metadata);
            var observer = TryGetBindingPathObserverInternal(source, bindingPath, metadata);
            if (observer == null)
                ExceptionManager.ThrowNotSupported(nameof(IBindingPathObserver));
            return observer;
        }

        #endregion

        #region Methods

        protected virtual bool TryGetMemberObserverInternal(Type type, object member, IReadOnlyMetadataContext metadata, out BindingMemberObserver observer)
        {
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i].TryGetMemberObserver(type, member, metadata, out observer))
                    return true;
            }

            observer = default;
            return false;
        }

        protected virtual IBindingPath? TryGetBindingPathInternal(object path, IReadOnlyMetadataContext metadata)
        {
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] is IBindingPathChildBindingObserverProvider provider)
                {
                    var bindingPath = provider.TryGetBindingPath(path, metadata);
                    if (bindingPath != null)
                        return bindingPath;
                }
            }

            return null;
        }

        protected virtual IBindingPathObserver? TryGetBindingPathObserverInternal(object source, IBindingPath path, IReadOnlyMetadataContext metadata)
        {
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] is IBindingPathChildBindingObserverProvider provider)
                {
                    var observer = provider.TryGetBindingPathObserver(source, path, metadata);
                    if (observer != null)
                        return observer;
                }
            }

            return null;
        }

        #endregion
    }
}