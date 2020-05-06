using System;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Components.Internal
{
    public class TestAttachableComponent<T> : AttachableComponentBase<T>, IComponent where T : class
    {
        #region Properties

        public Action<T, IReadOnlyMetadataContext?>? OnAttached { get; set; }

        public Action<T, IReadOnlyMetadataContext?>? OnDetached { get; set; }

        public Func<T, IReadOnlyMetadataContext?, bool>? OnAttaching { get; set; }

        public Func<T, IReadOnlyMetadataContext?, bool>? OnDetaching { get; set; }

        public new T Owner => base.Owner;

        public new bool IsAttached => base.IsAttached;

        #endregion

        #region Methods

        protected override void OnAttachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            OnAttached?.Invoke(owner, metadata);
            base.OnAttachedInternal(owner, metadata);
        }

        protected override bool OnAttachingInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            return OnAttaching?.Invoke(owner, metadata) ?? base.OnAttachingInternal(owner, metadata);
        }

        protected override void OnDetachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            OnDetached?.Invoke(owner, metadata);
            base.OnDetachedInternal(owner, metadata);
        }

        protected override bool OnDetachingInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            return OnDetaching?.Invoke(owner, metadata) ?? base.OnDetachingInternal(owner, metadata);
        }

        #endregion
    }
}