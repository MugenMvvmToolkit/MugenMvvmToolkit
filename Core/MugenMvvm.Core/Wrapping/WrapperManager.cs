using System;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;

namespace MugenMvvm.Wrapping
{
    public sealed class WrapperManager : ComponentOwnerBase<IWrapperManager>, IWrapperManager
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public WrapperManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool CanWrap(Type type, Type wrapperType, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            if (wrapperType.IsAssignableFrom(type))
                return true;

            var components = GetComponents<IWrapperManagerComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].CanWrap(this, type, wrapperType, metadata))
                    return true;
            }

            return false;
        }

        public object Wrap(object item, Type wrapperType, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            object? wrapper = null;
            var components = GetComponents<IWrapperManagerComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                wrapper = components[i].TryWrap(this, item.GetType(), wrapperType, metadata);
                if (wrapper != null)
                    break;
            }

            if (wrapper == null)
                ExceptionManager.ThrowWrapperTypeNotSupported(wrapperType);

            var listeners = GetComponents<IWrapperManagerListener>(metadata);
            for (var i = 0; i < components.Length; i++)
                listeners[i].OnWrapped(this, wrapper!, item, wrapperType, metadata);

            return wrapper;
        }

        #endregion
    }
}