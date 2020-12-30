using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace MugenMvvm.Internal
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

        public ActionToken(ItemOrList<ActionToken, IReadOnlyList<ActionToken>> tokens)
        {
            if (tokens.HasItem)
            {
                var token = tokens.Item;
                _handler = token._handler;
                _state1 = token._state1;
                _state2 = token._state2;
            }
            else if (tokens.List != null)
            {
                _handler = new Action<object?, object?>((o, _) =>
                {
                    var list = (ActionToken[]) o!;
                    for (var i = 0; i < list.Length; i++)
                        list[i].Dispose();
                });
                _state1 = tokens.List;
                _state2 = null;
            }
            else
            {
                _handler = null;
                _state1 = null;
                _state2 = null;
            }
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

        public static ActionToken NoDoToken => new((_, __) => { });

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
                ((Action<object?, object?>) handler).Invoke(_state1, _state2);
            _state1 = null;
            _state2 = null;
        }

        #endregion

        #region Methods

        public void Deconstruct(out object? handler, out object? state1, out object? state2)
        {
            handler = _handler;
            state1 = _state1;
            state2 = _state2;
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