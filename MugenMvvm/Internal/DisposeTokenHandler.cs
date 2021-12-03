using System;
using System.Runtime.InteropServices;
using MugenMvvm.Collections;

// ReSharper disable InconsistentlySynchronizedField

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public struct DisposeTokenHandler : IDisposable
    {
        private readonly object _locker;
        private ListInternal<ActionToken> _disposeTokens;

        public DisposeTokenHandler(object locker)
        {
            Should.NotBeNull(locker, nameof(locker));
            _locker = locker;
            IsDisposed = false;
            _disposeTokens = default;
        }

        public bool IsDisposed { get; private set; }

        public void Register(IDisposable token) => Register(ActionToken.FromDisposable(token));

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
            lock (_locker)
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

        public bool TryDispose() => TryDispose<object?>(null, null);

        public bool TryDispose<TState>(TState state, Action<TState>? disposeAction)
        {
            if (IsDisposed)
                return false;
            lock (_locker)
            {
                if (IsDisposed)
                    return false;
                IsDisposed = true;
            }

            disposeAction?.Invoke(state);
            if (!_disposeTokens.IsEmpty)
            {
                for (var i = 0; i < _disposeTokens.Count; i++)
                    _disposeTokens.Items[i].Dispose();
                _disposeTokens = default;
            }

            return true;
        }

        public void Dispose() => TryDispose();
    }
}