using Java.Lang;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Android.Internal
{
    public sealed class AndroidWeakReferenceProvider : IWeakReferenceProviderComponent, IHasPriority
    {
        public int Priority { get; init; } = InternalComponentPriority.WeakReferenceProvider + 1;

        public IWeakReference? TryGetWeakReference(IWeakReferenceManager weakReferenceManager, object item, IReadOnlyMetadataContext? metadata)
        {
            if (item is Object t)
                return new AndroidWeakReference(t, true);
            return null;
        }
    }
}