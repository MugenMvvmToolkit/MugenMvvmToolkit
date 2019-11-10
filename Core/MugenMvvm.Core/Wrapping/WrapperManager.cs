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

            var components = Components.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IWrapperManagerComponent component && component.CanWrap(this, type, wrapperType, metadata))
                    return true;
            }

            return false;
        }

        public object Wrap(object item, Type wrapperType, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            object? wrapper = null;
            var components = Components.GetComponents();
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