using System;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public class TestWeakReferenceProviderComponent : IWeakReferenceProviderComponent, IHasPriority
    {
        private readonly IWeakReferenceManager? _weakReferenceManager;

        public TestWeakReferenceProviderComponent(IWeakReferenceManager? weakReferenceManager)
        {
            _weakReferenceManager = weakReferenceManager;
        }

        public Func<object, IReadOnlyMetadataContext?, IWeakReference?>? TryGetWeakReference { get; set; }

        public int Priority { get; set; }

        IWeakReference? IWeakReferenceProviderComponent.TryGetWeakReference(IWeakReferenceManager weakReferenceManager, object item, IReadOnlyMetadataContext? metadata)
        {
            _weakReferenceManager?.ShouldEqual(weakReferenceManager);
            return TryGetWeakReference?.Invoke(item, metadata);
        }
    }
}