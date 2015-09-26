using System.Collections.Generic;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class BusyTokenMock : IBusyToken
    {
        #region Fields

        private readonly List<IBusyTokenCallback> _handlers;

        #endregion

        #region Constructors

        public BusyTokenMock(object message = null)
        {
            _handlers = new List<IBusyTokenCallback>();
            Message = message;
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            IsCompleted = true;
            foreach (var handler in _handlers)
                handler.OnCompleted(this);
            _handlers.Clear();
        }

        public bool IsCompleted { get; private set; }

        public object Message { get; private set; }

        public void Register(IBusyTokenCallback callback)
        {
            _handlers.Add(callback);
        }

        #endregion
    }
}
