using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class WeakReferenceProvider : IWeakReferenceProviderComponent, IHasPriority
    {
        public bool TrackResurrection { get; set; }

        public int Priority { get; init; } = InternalComponentPriority.WeakReferenceProvider;

        public IWeakReference TryGetWeakReference(IWeakReferenceManager weakReferenceManager, object item, IReadOnlyMetadataContext? metadata) =>
            new WeakReferenceImpl(item, TrackResurrection);
    }
}