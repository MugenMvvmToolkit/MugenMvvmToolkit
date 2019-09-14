using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IDataBinding : IDisposable, IComponentOwner<IDataBinding>, IMetadataOwner<IReadOnlyMetadataContext>
    {
        DataBindingState State { get; }

        IBindingPathObserver Target { get; }

        ItemOrList<IBindingPathObserver, IBindingPathObserver[]> Source { get; }

        bool? GetFlag(IMetadataContextKey<bool> flag);

        bool SetFlag(IMetadataContextKey<bool> flag, bool value);

        void UpdateTarget();

        void UpdateSource();
    }
}