using System;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Components.Internal
{
    public class TestAttachableComponent<T> : AttachableComponentBase<T>, IComponent<T>, IHasAttachConditionComponent, IHasDetachConditionComponent where T : class
    {
        public Action<T, IReadOnlyMetadataContext?>? OnAttachedHandler { get; set; }

        public Action<T, IReadOnlyMetadataContext?>? OnDetachedHandler { get; set; }

        public Action<T, IReadOnlyMetadataContext?>? OnAttachingHandler { get; set; }

        public Action<T, IReadOnlyMetadataContext?>? OnDetachingHandler { get; set; }

        public Func<T, IReadOnlyMetadataContext?, bool>? CanAttach { get; set; }

        public Func<T, IReadOnlyMetadataContext?, bool>? CanDetach { get; set; }

        public new T Owner => base.Owner;

        public new bool IsAttached => base.IsAttached;

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

        bool IHasAttachConditionComponent.CanAttach(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (CanAttach != null && owner is T t)
                return CanAttach(t, metadata);
            return true;
        }

        bool IHasDetachConditionComponent.CanDetach(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (CanDetach != null && owner is T t)
                return CanDetach(t, metadata);
            return true;
        }
    }
}