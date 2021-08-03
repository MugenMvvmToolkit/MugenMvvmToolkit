using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;

// ReSharper disable InconsistentlySynchronizedField

namespace MugenMvvm.Internal.Components
{
    internal sealed class DisposeCallbackComponent<T> : IAttachableComponent, IHasDetachConditionComponent, IDisposableComponent<T> where T : class
    {
        private int _state;
        private ListInternal<ActionToken> _disposeTokens;

        [Preserve(Conditional = true)]
        public DisposeCallbackComponent()
        {
        }

        private bool IsDisposed => _state > 1;

        public void Register(ActionToken token)
        {
            if (token.IsEmpty)
                return;

            if (IsDisposed)
            {
                token.Dispose();
                return;
            }

            var inline = false;
            lock (this)
            {
                if (IsDisposed)
                    inline = true;
                else
                {
                    if (_disposeTokens.IsEmpty)
                        _disposeTokens = new ListInternal<ActionToken>(2);
                    _disposeTokens.Add(token);
                }
            }

            if (inline)
                token.Dispose();
        }

        void IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (Interlocked.Increment(ref _state) != 1)
                ExceptionManager.ThrowObjectInitialized(this);
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IDisposableComponent<T>.Dispose(T owner, IReadOnlyMetadataContext? metadata)
        {
            if (IsDisposed)
                return;
            lock (this)
            {
                if (IsDisposed)
                    return;
                Interlocked.Increment(ref _state);
            }

            if (!_disposeTokens.IsEmpty)
            {
                for (var i = 0; i < _disposeTokens.Count; i++)
                    _disposeTokens.Items[i].Dispose();
                _disposeTokens = default;
            }
        }

        bool IHasDetachConditionComponent.CanDetach(object owner, IReadOnlyMetadataContext? metadata) => false;
    }
}