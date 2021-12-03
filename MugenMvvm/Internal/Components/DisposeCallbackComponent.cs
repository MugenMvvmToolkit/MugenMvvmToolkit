using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;

// ReSharper disable InconsistentlySynchronizedField

namespace MugenMvvm.Internal.Components
{
    internal sealed class DisposeCallbackComponent<T> : IAttachableComponent, IHasDetachConditionComponent, IDisposableComponent<T> where T : class
    {
        private bool _isAttached;
        private DisposeTokenHandler _disposeTokenHandler;

        [Preserve(Conditional = true)]
        public DisposeCallbackComponent()
        {
            _disposeTokenHandler = new DisposeTokenHandler(this);
        }

        public void Register(ActionToken token) => _disposeTokenHandler.Register(token);

        void IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (_isAttached)
                ExceptionManager.ThrowObjectInitialized(this);
            _isAttached = true;
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IDisposableComponent<T>.OnDisposing(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IDisposableComponent<T>.OnDisposed(T owner, IReadOnlyMetadataContext? metadata) => _disposeTokenHandler.Dispose();

        bool IHasDetachConditionComponent.CanDetach(object owner, IReadOnlyMetadataContext? metadata) => false;
    }
}