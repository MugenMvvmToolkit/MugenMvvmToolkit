using Foundation;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Ios.Internal
{
    public sealed class IosWeakReferenceProvider : IWeakReferenceProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = InternalComponentPriority.WeakReferenceProvider + 1;

        #endregion

        #region Implementation of interfaces

        public IWeakReference? TryGetWeakReference(IWeakReferenceManager weakReferenceManager, object item, IReadOnlyMetadataContext? metadata)
        {
            if (item is NSObject nsObject)
                return IosAttachedValueHolder.Get(nsObject, false)!.WeakReference;
            return null;
        }

        #endregion
    }
}