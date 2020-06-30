using System;
using System.Diagnostics.CodeAnalysis;
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
        public WrapperManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool CanWrap<TRequest>(Type wrapperType, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IWrapperManagerComponent>(metadata).CanWrap(wrapperType, request, metadata);
        }

        public object? TryWrap<TRequest>(Type wrapperType, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            var wrapper = GetComponents<IWrapperManagerComponent>(metadata).TryWrap(wrapperType, request, metadata);
            if (wrapper != null)
                GetComponents<IWrapperManagerListener>(metadata).OnWrapped(this, wrapper, request, metadata);
            return wrapper;
        }

        #endregion
    }
}