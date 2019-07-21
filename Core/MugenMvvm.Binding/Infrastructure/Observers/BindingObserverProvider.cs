using System;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Infrastructure.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Observers
{
    public class BindingObserverProvider : ComponentOwnerBase<IBindingObserverProvider>, IBindingObserverProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public BindingObserverProvider(IComponentCollectionProvider componentCollectionProvider)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public BindingMemberObserver GetMemberObserver(Type type, object member, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetMemberObserverInternal(type, member, metadata);
        }

        public IBindingPath GetBindingPath(object path, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(metadata, nameof(metadata));
            var p = path as IBindingPath ?? TryGetBindingPathInternal(path, metadata);
            if (p == null)
                ExceptionManager.ThrowNotSupported(nameof(path));
            return p!;
        }

        public IBindingPathObserver GetBindingPathObserver(object source, object path, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(source, nameof(source));
            var bindingPath = GetBindingPath(path, metadata);
            var observer = TryGetBindingPathObserverInternal(source, bindingPath, metadata);
            if (observer == null)
                ExceptionManager.ThrowNotSupported(nameof(IBindingPathObserver));
            return observer!;
        }

        #endregion

        #region Methods

        protected virtual BindingMemberObserver GetMemberObserverInternal(Type type, object member, IReadOnlyMetadataContext metadata)
        {
            var items = Components.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] is IBindingMemberObserverProviderComponent component)
                {
                    var observer = component.TryGetMemberObserver(type, member, metadata);
                    if (!observer.IsEmpty)
                        return observer;
                }
            }

            return default;
        }

        protected virtual IBindingPath? TryGetBindingPathInternal(object path, IReadOnlyMetadataContext metadata)
        {
            var items = Components.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var bindingPath = (items[i] as IBindingPathProviderComponent)?.TryGetBindingPath(path, metadata);
                if (bindingPath != null)
                    return bindingPath;
            }

            return null;
        }

        protected virtual IBindingPathObserver? TryGetBindingPathObserverInternal(object source, IBindingPath path, IReadOnlyMetadataContext? metadata)
        {
            var items = Components.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var observer = (items[i] as IBindingPathObserverProviderComponent)?.TryGetBindingPathObserver(source, path, metadata);
                if (observer != null)
                    return observer;
            }

            return null;
        }

        #endregion
    }
}