using System;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class WeakReferenceProviderComponent : IWeakReferenceProviderComponent, IHasPriority
    {
        #region Properties

        public bool TrackResurrection { get; set; }

        public int Priority { get; set; } = InternalComponentPriority.WeakReferenceProvider;

        #endregion

        #region Implementation of interfaces

        public IWeakReference? TryGetWeakReference(object item, IReadOnlyMetadataContext? metadata)
        {
            return new WeakReferenceImpl(item, TrackResurrection);
        }

        #endregion

        #region Nested types

        private sealed class WeakReferenceImpl : WeakReference, IWeakReference
        {
            #region Constructors

            public WeakReferenceImpl(object target, bool trackResurrection) : base(target, trackResurrection)
            {
            }

            #endregion

            #region Implementation of interfaces

            public void Release()
            {
                Target = null;
            }

            #endregion
        }

        #endregion
    }
}