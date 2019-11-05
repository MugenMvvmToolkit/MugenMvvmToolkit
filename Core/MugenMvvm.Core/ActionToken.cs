using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MugenMvvm
{
    [StructLayout(LayoutKind.Auto)]
    public struct ActionToken : IDisposable
    {
        #region Fields

        private object? _handler;
        private object? _state1;
        private object? _state2;

        #endregion

        #region Constructors

        private ActionToken(object handler, object? state, object? state2)
        {
            Should.NotBeNull(handler, nameof(handler));
            _handler = handler;
            _state1 = state;
            _state2 = state2;
        }

        public ActionToken(IHandler handler, object? state1 = null, object? state2 = null) : this(handler, state: state1, state2)
        {
        }

        public ActionToken(Action<object?, object?> handler, object? state1 = null, object? state2 = null) : this(handler, state: state1, state2)
        {
        }

        #endregion

        #region Properties

        public readonly bool IsEmpty => _handler == null;

        public static ActionToken NoDoToken => new ActionToken((_, __) => { }, null);

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (_handler == null)
                return;

            var handler = Interlocked.Exchange(ref _handler, null);
            if (handler == null)
                return;

            if (handler is IHandler h)
                h.Invoke(_state1, _state2);
            else
                ((Action<object?, object?>)handler).Invoke(_state1, _state2);
            _state1 = null;
            _state2 = null;
        }

        #endregion

        #region Nested types

        public interface IHandler
        {
            void Invoke(object? state1, object? state2);
        }

        #endregion
    }
}