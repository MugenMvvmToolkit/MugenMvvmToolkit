using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTests.Bindings.Core.Internal
{
    public class TestBinding : ComponentOwnerBase<IValidator>, IBinding
    {
        #region Constructors

        public TestBinding() : base(null)
        {
        }

        #endregion

        #region Properties

        public bool HasMetadata { get; set; }

        public IReadOnlyMetadataContext Metadata { get; set; } = null!;

        public BindingState State { get; set; } = null!;

        public IMemberPathObserver Target { get; set; } = null!;

        public ItemOrArray<object?> Source { get; set; }

        public Action? Dispose { get; set; }

        public Func<ItemOrArray<object>>? GetComponents { get; set; }

        public Action? UpdateTarget { get; set; }

        public Action? UpdateSource { get; set; }

        #endregion

        #region Implementation of interfaces

        void IDisposable.Dispose() => Dispose?.Invoke();

        ItemOrArray<object> IBinding.GetComponents() => GetComponents?.Invoke() ?? default;

        void IBinding.UpdateTarget() => UpdateTarget?.Invoke();

        void IBinding.UpdateSource() => UpdateSource?.Invoke();

        #endregion
    }
}