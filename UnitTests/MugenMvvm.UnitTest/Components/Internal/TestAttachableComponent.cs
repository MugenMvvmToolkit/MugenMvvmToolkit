using System;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Components.Internal
{
    public class TestAttachableComponent<T> : AttachableComponentBase<T>, IComponent<T> where T : class
    {
        #region Properties

        public Action<T, IReadOnlyMetadataContext?>? OnAttachedHandler { get; set; }

        public Action<T, IReadOnlyMetadataContext?>? OnDetachedHandler { get; set; }

        public Func<T, IReadOnlyMetadataContext?, bool>? OnAttachingHandler { get; set; }

        public Func<T, IReadOnlyMetadataContext?, bool>? OnDetachingHandler { get; set; }

        public new T Owner => base.Owner;

        public new bool IsAttached => base.IsAttached;

        #endregion

        #region Methods

        protected override void OnAttached(T owner, IReadOnlyMetadataContext? metadata)
        {
            OnAttachedHandler?.Invoke(owner, metadata);
            base.OnAttached(owner, metadata);
        }

        protected override bool OnAttaching(T owner, IReadOnlyMetadataContext? metadata)
        {
            return OnAttachingHandler?.Invoke(owner, metadata) ?? base.OnAttaching(owner, metadata);
        }

        protected override void OnDetached(T owner, IReadOnlyMetadataContext? metadata)
        {
            OnDetachedHandler?.Invoke(owner, metadata);
            base.OnDetached(owner, metadata);
        }

        protected override bool OnDetaching(T owner, IReadOnlyMetadataContext? metadata)
        {
            return OnDetachingHandler?.Invoke(owner, metadata) ?? base.OnDetaching(owner, metadata);
        }

        #endregion
    }
}