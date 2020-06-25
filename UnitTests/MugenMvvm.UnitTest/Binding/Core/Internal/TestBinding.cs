using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
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

        public ItemOrList<object?, object?[]> Source { get; set; }

        public Action? Dispose { get; set; }

        public Func<ItemOrList<object, object[]>>? GetComponents { get; set; }

        public Action? UpdateTarget { get; set; }

        public Action? UpdateSource { get; set; }

        #endregion

        #region Implementation of interfaces

        void IDisposable.Dispose()
        {
            Dispose?.Invoke();
        }

        ItemOrList<object, object[]> IBinding.GetComponents()
        {
            return GetComponents?.Invoke() ?? default;
        }

        void IBinding.UpdateTarget()
        {
            UpdateTarget?.Invoke();
        }

        void IBinding.UpdateSource()
        {
            UpdateSource?.Invoke();
        }

        #endregion
    }
}