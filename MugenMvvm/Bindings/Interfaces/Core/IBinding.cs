using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Interfaces.Core
{
    public interface IBinding : IDisposable, IComponentOwner<IBinding>, IMetadataOwner<IReadOnlyMetadataContext>
    {
        BindingState State { get; }

        IMemberPathObserver Target { get; }

        ItemOrArray<object?> Source { get; }

        ItemOrArray<object> GetComponents();

        void UpdateTarget();

        void UpdateSource();
    }
}