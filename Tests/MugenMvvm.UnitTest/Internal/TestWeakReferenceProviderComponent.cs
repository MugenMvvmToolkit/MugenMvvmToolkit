using System;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Internal
{
    public class TestWeakReferenceProviderComponent : IWeakReferenceProviderComponent, IHasPriority
    {
        #region Properties

        public Func<object, IReadOnlyMetadataContext?, IWeakReference?>? TryGetWeakReference { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IWeakReference? IWeakReferenceProviderComponent.TryGetWeakReference(object item, IReadOnlyMetadataContext? metadata)
        {
            return TryGetWeakReference?.Invoke(item, metadata);
        }

        #endregion
    }
}