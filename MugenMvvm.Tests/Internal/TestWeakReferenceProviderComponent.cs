using System;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Internal
{
    public class TestWeakReferenceProviderComponent : IWeakReferenceProviderComponent, IHasPriority
    {
        public Func<IWeakReferenceManager, object, IReadOnlyMetadataContext?, IWeakReference?>? TryGetWeakReference { get; set; }

        public int Priority { get; set; }

        IWeakReference? IWeakReferenceProviderComponent.TryGetWeakReference(IWeakReferenceManager weakReferenceManager, object item, IReadOnlyMetadataContext? metadata) =>
            TryGetWeakReference?.Invoke(weakReferenceManager, item, metadata);
    }
}