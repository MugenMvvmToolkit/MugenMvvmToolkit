using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Tests.Bindings.Core
{
    public class TestBinding : ComponentOwnerBase<IBinding>, IBinding
    {
        public TestBinding(IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager)
        {
        }

        public Action? Dispose { get; set; }

        public Action? UpdateTarget { get; set; }

        public Action? UpdateSource { get; set; }

        public BindingState State { get; set; } = null!;

        public IMemberPathObserver Target { get; set; } = null!;

        public ItemOrArray<object?> Source { get; set; }

        public bool HasMetadata { get; set; }

        public IReadOnlyMetadataContext Metadata { get; set; } = null!;

        ItemOrArray<object> IBinding.GetComponents() => GetComponents<object>();

        void IBinding.UpdateTarget() => UpdateTarget?.Invoke();

        void IBinding.UpdateSource() => UpdateSource?.Invoke();

        void IDisposable.Dispose() => Dispose?.Invoke();
    }
}