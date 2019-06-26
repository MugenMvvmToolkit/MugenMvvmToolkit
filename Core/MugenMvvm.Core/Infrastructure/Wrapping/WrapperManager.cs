using System;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;

namespace MugenMvvm.Infrastructure.Wrapping
{
    public class WrapperManager : ComponentOwnerBase<IWrapperManager>, IWrapperManager
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public WrapperManager(IComponentCollectionProvider componentCollectionProvider)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool CanWrap(Type type, Type wrapperType, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            return CanWrapInternal(type, wrapperType, metadata);
        }

        public object Wrap(object item, Type wrapperType, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            return WrapInternal(item, wrapperType, metadata);
        }

        #endregion

        #region Methods

        protected virtual bool CanWrapInternal(Type type, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            if (wrapperType.IsAssignableFromUnified(type))
                return true;

            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IWrapperManagerComponent component && component.CanWrap(this, type, wrapperType, metadata))
                    return true;
            }

            return false;
        }

        protected virtual object WrapInternal(object item, Type wrapperType, IReadOnlyMetadataContext? metadata)
        {
            object? wrapper = null;
            var components = Components.GetItems();
            for (var i = 0; i < components.Length; i++)
            {
                wrapper = (components[i] as IWrapperManagerComponent)?.TryWrap(this, item.GetType(), wrapperType, metadata);
                if (wrapper != null)
                    break;
            }

            if (wrapper == null)
                ExceptionManager.ThrowWrapperTypeNotSupported(wrapperType);

            for (var i = 0; i < components.Length; i++)
                (components[i] as IWrapperManagerListener)?.OnWrapped(this, wrapper!, item, wrapperType, metadata);

            return wrapper!;
        }

        #endregion
    }
}