using System;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Components.Internal
{
    public class TestMultiAttachableComponent<T> : MultiAttachableComponentBase<T>, IComponent<T> where T : class
    {
        public Action<T, IReadOnlyMetadataContext?>? OnAttachedHandler { get; set; }

        public Action<T, IReadOnlyMetadataContext?>? OnDetachedHandler { get; set; }

        public Action<T, IReadOnlyMetadataContext?>? OnAttachingHandler { get; set; }

        public Action<T, IReadOnlyMetadataContext?>? OnDetachingHandler { get; set; }

        public new ItemOrArray<T> Owners => base.Owners;

        protected override void OnAttached(T owner, IReadOnlyMetadataContext? metadata)
        {
            OnAttachedHandler?.Invoke(owner, metadata);
            base.OnAttached(owner, metadata);
        }

        protected override void OnAttaching(T owner, IReadOnlyMetadataContext? metadata)
        {
            OnAttachingHandler?.Invoke(owner, metadata);
            base.OnAttaching(owner, metadata);
        }

        protected override void OnDetached(T owner, IReadOnlyMetadataContext? metadata)
        {
            OnDetachedHandler?.Invoke(owner, metadata);
            base.OnDetached(owner, metadata);
        }

        protected override void OnDetaching(T owner, IReadOnlyMetadataContext? metadata)
        {
            OnDetachingHandler?.Invoke(owner, metadata);
            base.OnDetaching(owner, metadata);
        }
    }
}