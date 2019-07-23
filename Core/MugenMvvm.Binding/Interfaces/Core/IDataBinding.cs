using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IDataBinding : IDisposable, IComponentOwner<IDataBinding>
    {
        DataBindingState State { get; }

        IBindingPathObserver Target { get; }

        IBindingPathObserver[] Sources { get; }

        void UpdateTarget();

        void UpdateSource();
    }
}