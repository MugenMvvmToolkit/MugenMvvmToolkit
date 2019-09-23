using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IBinding : IDisposable, IComponentOwner<IBinding>, IMetadataOwner<IReadOnlyMetadataContext>
    {
        BindingState State { get; }

        IMemberPathObserver Target { get; }

        ItemOrList<IMemberPathObserver, IMemberPathObserver[]> Source { get; }

        void UpdateTarget();

        void UpdateSource();
    }
}