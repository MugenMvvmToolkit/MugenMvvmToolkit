using System;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
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
            return GetComponents<IWrapperManagerComponent>(metadata).CanWrap(type, wrapperType, metadata);
        }

        public object Wrap(object item, Type wrapperType, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            var wrapper = GetComponents<IWrapperManagerComponent>(metadata).TryWrap(item.GetType(), wrapperType, metadata);
            if (wrapper == null)
                ExceptionManager.ThrowWrapperTypeNotSupported(wrapperType);

            GetComponents<IWrapperManagerListener>(metadata).OnWrapped(this, wrapper!, item, wrapperType, metadata);
            return wrapper;
        }

        #endregion
    }
}