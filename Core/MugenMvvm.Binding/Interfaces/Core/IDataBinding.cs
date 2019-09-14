using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IDataBinding : IDisposable, IComponentOwner<IDataBinding>
    {
        DataBindingState State { get; }

        IBindingPathObserver Target { get; }

        ItemOrList<IBindingPathObserver, IBindingPathObserver[]> Source { get; }

        bool TryGet<T>(IMetadataContextKey<T> key, out T value);

        bool Set<T>(IMetadataContextKey<T> key, T value);

        void UpdateTarget();

        void UpdateSource();
    }
}