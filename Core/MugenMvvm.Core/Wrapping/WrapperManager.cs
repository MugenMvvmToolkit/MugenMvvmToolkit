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

        public bool CanWrap(Type targetType, Type wrapperType, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IWrapperManagerComponent>(metadata).CanWrap(targetType, wrapperType, metadata);
        }

        public object Wrap(object target, Type wrapperType, IReadOnlyMetadataContext? metadata = null)
        {
            var wrapper = GetComponents<IWrapperManagerComponent>(metadata).TryWrap(target, wrapperType, metadata);
            if (wrapper == null)
                ExceptionManager.ThrowWrapperTypeNotSupported(wrapperType);

            GetComponents<IWrapperManagerListener>(metadata).OnWrapped(this, wrapper!, target, wrapperType, metadata);
            return wrapper;
        }

        #endregion
    }
}