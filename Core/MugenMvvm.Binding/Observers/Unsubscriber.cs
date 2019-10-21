using System;
using System.Runtime.InteropServices;

namespace MugenMvvm.Binding.Observers
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct Unsubscriber
    {
        #region Fields

        public static readonly Unsubscriber NoDoUnsubscriber = new Unsubscriber((o, o1) => { }, null, null);

        private readonly object? _handler;
        private readonly object? _state1;
        private readonly object? _state2;

        #endregion

        #region Constructors

        private Unsubscriber(object handler, object? state, object? state2)
        {
            Should.NotBeNull(handler, nameof(handler));
            _handler = handler;
            _state1 = state;
            _state2 = state2;
        }

        public Unsubscriber(IHandler handler, object? state1, object? state2) : this(handler, state: state1, state2)
        {
        }

        public Unsubscriber(Action<object, object> handler, object? state1, object? state2) : this(handler, state: state1, state2)
        {
        }

        #endregion

        #region Properties

        public bool IsEmpty => _handler == null;

        #endregion

        #region Methods

        public void Unsubscribe()
        {
            if (_handler == null)
                return;
            if (_handler is IHandler handler)
                handler.Unsubscribe(_state1, _state2);
            else
                ((Action<object, object>) _handler).Invoke(_state1, _state2);
        }

        #endregion

        #region Nested types

        public interface IHandler
        {
            void Unsubscribe(object? state1, object? state2);
        }

        #endregion
    }
}