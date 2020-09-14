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
        #region Fields

        private readonly IWeakReferenceManager? _weakReferenceManager;

        #endregion

        #region Constructors

        public TestWeakReferenceProviderComponent(IWeakReferenceManager? weakReferenceManager)
        {
            _weakReferenceManager = weakReferenceManager;
        }

        #endregion

        #region Properties

        public Func<object, IReadOnlyMetadataContext?, IWeakReference?>? TryGetWeakReference { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IWeakReference? IWeakReferenceProviderComponent.TryGetWeakReference(IWeakReferenceManager weakReferenceManager, object item, IReadOnlyMetadataContext? metadata)
        {
            _weakReferenceManager?.ShouldEqual(weakReferenceManager);
            return TryGetWeakReference?.Invoke(item, metadata);
        }

        #endregion
    }
}